using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions;
using SharedKernel;

namespace Platform.Infrastructure.Persistence;

internal sealed class UnitOfWorkFactory(IEnumerable<ModuleDbContext> contexts, DispatchContext dispatch, TimeProvider time) : IUnitOfWorkFactory
{
    public IUnitOfWork For(Type messageType) => new UnitOfWork(ContextOf(messageType), dispatch, time);

    public ModuleDbContext ContextOf(Type type)
    {
        var module = type.Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "Module")?.Value;
        return contexts.SingleOrDefault(c => c.Module == module)
            ?? throw new InvalidOperationException($"{type.FullName} belongs to module '{module}', which registers no ModuleDbContext.");
    }
}

/// <summary>The adapter: the one place an exception EF raises for an expected outcome becomes an outcome.</summary>
internal sealed class UnitOfWork(ModuleDbContext db, DispatchContext dispatch, TimeProvider time) : IUnitOfWork
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public bool HasChanges => db.ChangeTracker.HasChanges();

    public IReadOnlyList<long> LoadedVersions =>
        db.ChangeTracker.Entries<IAggregateRoot>()
            .Where(e => e.State is not (EntityState.Added or EntityState.Detached))
            .Select(e => (long)e.OriginalValues[nameof(IAggregateRoot.Version)]!)
            .ToList();

    public bool ChangesLoadedAggregates =>
        db.ChangeTracker.Entries<IAggregateRoot>().Any(e => e.State is EntityState.Modified or EntityState.Deleted);

    public async Task<RecordedResult?> FindAttempt(AttemptIdentity attempt, CancellationToken cancellationToken)
    {
        var record = await db.Set<IdempotencyRecord>().AsNoTracking().SingleOrDefaultAsync(
            r => r.PrincipalId == attempt.PrincipalId && r.OperationId == attempt.OperationId && r.Key == attempt.Key,
            cancellationToken);

        return record is null
            ? null
            : new RecordedResult(
                record.Fingerprint,
                record.Succeeded,
                record.ValueJson,
                record.ProblemType is null ? null : new ProblemType(record.ProblemType, record.ProblemStatus!.Value, record.ProblemTitle!, record.ProblemUserFacing!.Value),
                record.ProblemDetail,
                record.ProblemErrorsJson is null ? null : JsonSerializer.Deserialize<Dictionary<string, string[]>>(record.ProblemErrorsJson));
    }

    public void StageAttempt(AttemptIdentity attempt, RecordedResult result) =>
        db.Set<IdempotencyRecord>().Add(new IdempotencyRecord
        {
            PrincipalId = attempt.PrincipalId,
            OperationId = attempt.OperationId,
            Key = attempt.Key,
            Fingerprint = result.Fingerprint,
            Succeeded = result.Succeeded,
            ValueJson = result.ValueJson,
            ProblemType = result.ProblemType?.Uri,
            ProblemStatus = result.ProblemType?.Status,
            ProblemTitle = result.ProblemType?.Title,
            ProblemUserFacing = result.ProblemType?.UserFacing,
            ProblemDetail = result.Detail,
            ProblemErrorsJson = result.Errors is null ? null : JsonSerializer.Serialize(result.Errors),
            CreatedAt = time.GetUtcNow(),
        });

    public void StageAudit(AuditEntry entry) =>
        db.Set<AuditRecord>().Add(new AuditRecord
        {
            At = entry.At,
            PrincipalId = entry.PrincipalId,
            OperationId = entry.OperationId,
            Outcome = entry.Outcome.ToString(),
            ProblemType = entry.ProblemType,
            RecordIds = string.Join(',', entry.RecordIds),
            TraceId = entry.TraceId,
        });

    public async Task<CommitOutcome> Commit(CancellationToken cancellationToken)
    {
        StageDomainEvents();
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return CommitOutcome.Committed;
        }
        catch (DbUpdateConcurrencyException)
        {
            return CommitOutcome.Conflict;
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation })
        {
            return CommitOutcome.UniqueViolation;
        }
    }

    public void Discard() => db.ChangeTracker.Clear();

    private void StageDomainEvents()
    {
        foreach (var root in db.ChangeTracker.Entries<IAggregateRoot>().Select(e => e.Entity).ToList())
        {
            foreach (var domainEvent in root.DomainEvents)
            {
                db.Set<OutboxMessage>().Add(OutboxRows.For(domainEvent, dispatch, time));
            }

            root.ClearDomainEvents();
        }
    }
}

internal static class OutboxRows
{
    public static OutboxMessage For(object @event, DispatchContext dispatch, TimeProvider time) => new()
    {
        Id = Guid.CreateVersion7(),
        Type = @event.GetType().AssemblyQualifiedName!,
        Payload = JsonSerializer.Serialize(@event, @event.GetType()),
        OccurredAt = time.GetUtcNow(),
        TraceParent = Activity.Current?.Id is { Length: 55 } id ? id : $"00-{dispatch.TraceId}-{ActivitySpanId.CreateRandom().ToHexString()}-01",
        TraceState = Activity.Current?.TraceStateString,
        InitiatingPrincipalId = dispatch.PrincipalId,
    };
}

/// <summary>Integration events go to the outbox of the module whose Contracts declares them, with the change.</summary>
internal sealed class Outbox(UnitOfWorkFactory units, DispatchContext dispatch, TimeProvider time) : IOutbox
{
    public void Publish(IIntegrationEvent integrationEvent) =>
        units.ContextOf(integrationEvent.GetType()).Set<OutboxMessage>().Add(OutboxRows.For(integrationEvent, dispatch, time));
}
