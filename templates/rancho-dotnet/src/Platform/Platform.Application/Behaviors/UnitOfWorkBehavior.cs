using System.Security.Cryptography;
using System.Text.Json;
using Mediator;
using Platform.Application.Abstractions;

namespace Platform.Application.Behaviors;

/// <summary>
/// Last in the pipeline, commands only: the unit of work and idempotency, as one behavior.
/// The aggregate, its outbox rows, the idempotency record, and the audit record commit in one transaction.
/// </summary>
public sealed class UnitOfWorkBehavior<TMessage, TResponse>(IUnitOfWorkFactory units, DispatchContext context, TimeProvider time)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage, IBaseCommand, IDeclaresOperation
    where TResponse : IResult<TResponse>
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = DispatchTelemetry.StartStep("unit-of-work", GetType());
        var operation = TMessage.Operation;
        var unit = units.For(typeof(TMessage));

        if (operation.Transaction == TransactionRequirement.None)
        {
            return await WithoutTransaction(message, next, unit, cancellationToken);
        }

        AttemptIdentity? attempt = null;
        var fingerprint = string.Empty;
        if (operation.Idempotency == IdempotencyMechanism.Key)
        {
            if (context.IdempotencyKey is not { } key)
            {
                return TResponse.Failure(PlatformProblems.IdempotencyKeyRequired.With());
            }

            attempt = new AttemptIdentity(context.PrincipalId, operation.Id, key);
            fingerprint = Fingerprint(message);
            if (await unit.FindAttempt(attempt, cancellationToken) is { } prior)
            {
                return Answer(prior, fingerprint);
            }
        }

        TResponse result;
        try
        {
            result = await next(message, cancellationToken);
        }
        catch
        {
            unit.Discard();
            throw;
        }

        if (message is IVersionedCommand versioned)
        {
            // The precondition is evaluated before anything is written, whatever the handler decided.
            if (unit.LoadedVersions.Any(v => !versioned.ExpectedVersion.Matches(v)))
            {
                result = TResponse.Failure(PlatformProblems.PreconditionFailed.With());
            }
        }
        else if (unit.ChangesLoadedAggregates)
        {
            unit.Discard();
            throw new InvalidOperationException(
                $"{operation.Id} changed an aggregate it loaded, but its message is not an IVersionedCommand. A write to an existing resource carries If-Match.");
        }

        if (result.IsSuccess)
        {
            if (attempt is not null)
            {
                unit.StageAttempt(attempt, result.Record(fingerprint));
            }

            unit.StageAudit(Audit.Entry(message, operation, context, time, AuditOutcome.Succeeded, null));
            var committed = await unit.Commit(cancellationToken);
            if (committed == CommitOutcome.Committed)
            {
                return result;
            }

            unit.Discard();
            if (attempt is not null && await unit.FindAttempt(attempt, cancellationToken) is { } winner)
            {
                // A concurrent duplicate committed first: this attempt rolls back and answers as its replay.
                return Answer(winner, fingerprint);
            }

            // Someone changed the aggregate between this load and this commit.
            result = TResponse.Failure(message is IVersionedCommand ? PlatformProblems.PreconditionFailed.With() : PlatformProblems.Conflict.With());
        }

        // A failure: nothing the handler staged survives. The failure is recorded on its own, so a replay returns it.
        unit.Discard();
        if (attempt is not null)
        {
            unit.StageAttempt(attempt, result.Record(fingerprint));
        }

        unit.StageAudit(Audit.Entry(message, operation, context, time, AuditOutcome.Failed, result.Problem));
        if (await unit.Commit(cancellationToken) == CommitOutcome.UniqueViolation && attempt is not null)
        {
            unit.Discard();
            if (await unit.FindAttempt(attempt, cancellationToken) is { } winner)
            {
                return Answer(winner, fingerprint);
            }
        }

        return result;
    }

    private async ValueTask<TResponse> WithoutTransaction(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, IUnitOfWork unit, CancellationToken cancellationToken)
    {
        var result = await next(message, cancellationToken);
        if (unit.HasChanges)
        {
            unit.Discard();
            throw new InvalidOperationException(
                $"{TMessage.Operation.Id} declares Transaction None but staged a database change. A command that persists declares Required.");
        }

        unit.StageAudit(Audit.Entry(message, TMessage.Operation, context, time, result.IsSuccess ? AuditOutcome.Succeeded : AuditOutcome.Failed, result.Problem));
        await unit.Commit(cancellationToken);
        return result;
    }

    private static TResponse Answer(RecordedResult prior, string fingerprint) =>
        prior.Fingerprint == fingerprint
            ? TResponse.Replay(prior)
            : TResponse.Failure(PlatformProblems.IdempotencyKeyReused.With());

    private static string Fingerprint(TMessage message) =>
        Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(message)));
}
