namespace Platform.Application.Abstractions;

/// <summary>
/// The module's unit of work, as the pipeline sees it. Only the pipeline's behaviors use these ports;
/// a module's handlers save through their repositories and publish through <see cref="IOutbox"/>.
/// </summary>
public interface IUnitOfWorkFactory
{
    /// <summary>The unit of work of the module that owns <paramref name="messageType"/>.</summary>
    IUnitOfWork For(Type messageType);
}

public interface IUnitOfWork
{
    /// <summary>Whether the module's context holds a staged change.</summary>
    bool HasChanges { get; }

    /// <summary>The versions, as loaded, of the aggregates the handler read from the database.</summary>
    IReadOnlyList<long> LoadedVersions { get; }

    /// <summary>Whether the handler changed an aggregate it loaded, rather than only adding new ones.</summary>
    bool ChangesLoadedAggregates { get; }

    Task<RecordedResult?> FindAttempt(AttemptIdentity attempt, CancellationToken cancellationToken);

    void StageAttempt(AttemptIdentity attempt, RecordedResult result);

    void StageAudit(AuditEntry entry);

    /// <summary>
    /// Commits everything staged in one transaction: the aggregates, their domain events as outbox rows,
    /// the idempotency record, and the audit record. A conflict is an outcome, never an exception.
    /// </summary>
    Task<CommitOutcome> Commit(CancellationToken cancellationToken);

    /// <summary>Drops everything staged: the rollback.</summary>
    void Discard();
}

public enum CommitOutcome
{
    Committed,

    /// <summary>A concurrency token moved: someone else changed the aggregate first.</summary>
    Conflict,

    /// <summary>A unique index refused the write, such as a second attempt with the same idempotency key.</summary>
    UniqueViolation,
}

/// <summary>Who tried what, under which key. Unique per module.</summary>
public sealed record AttemptIdentity(string PrincipalId, string OperationId, string Key);

/// <summary>What the first attempt answered, so a replay answers the same.</summary>
public sealed record RecordedResult(
    string Fingerprint,
    bool Succeeded,
    string? ValueJson,
    ProblemType? ProblemType = null,
    string? Detail = null,
    IReadOnlyDictionary<string, string[]>? Errors = null);

/// <summary>One line of the audit trail. It holds ids, never values.</summary>
public sealed record AuditEntry(
    DateTimeOffset At,
    string PrincipalId,
    string OperationId,
    AuditOutcome Outcome,
    string? ProblemType,
    IReadOnlyList<Guid> RecordIds,
    string TraceId);

public enum AuditOutcome
{
    Succeeded,
    Failed,
    Denied,
}

/// <summary>How a handler tells other modules something happened: the event is written to the outbox with the change.</summary>
public interface IOutbox
{
    void Publish(IIntegrationEvent integrationEvent);
}
