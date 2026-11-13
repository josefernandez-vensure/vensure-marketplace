namespace Platform.Application.Abstractions;

public enum OperationKind
{
    Command,
    Query,
}

public enum IdempotencyMechanism
{
    /// <summary>Repeating it changes nothing further.</summary>
    Natural,

    /// <summary>The client supplies the id of what it creates; a repeat finds it.</summary>
    ClientSuppliedId,

    /// <summary>The client sends an Idempotency-Key header; a replay returns the recorded result.</summary>
    Key,
}

public enum TransactionRequirement
{
    Required,

    /// <summary>The effect is an external write; the command stages no database change.</summary>
    None,
}

public static class RateLimits
{
    public const string Default = "default";
}

/// <summary>
/// What the pipeline needs to know about a message, declared once in its slice. The two factories are the only way to build one,
/// so a query can never ask for a transaction or a key.
/// </summary>
public sealed record Operation
{
    private Operation(string id, string policy, OperationKind kind, IdempotencyMechanism idempotency, TransactionRequirement transaction, string rateLimit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(policy);
        ArgumentException.ThrowIfNullOrWhiteSpace(rateLimit);
        Id = id;
        Policy = policy;
        Kind = kind;
        Idempotency = idempotency;
        Transaction = transaction;
        RateLimit = rateLimit;
    }

    /// <summary><c>&lt;module&gt;.&lt;operation&gt;</c>, such as <c>catalog.rename-product</c>. It is also the OpenAPI operationId.</summary>
    public string Id { get; }

    public string Policy { get; }

    public OperationKind Kind { get; }

    public IdempotencyMechanism Idempotency { get; }

    public TransactionRequirement Transaction { get; }

    public string RateLimit { get; }

    public static Operation Command(
        string id,
        string policy,
        IdempotencyMechanism idempotency,
        TransactionRequirement transaction = TransactionRequirement.Required,
        string rateLimit = RateLimits.Default)
    {
        if (transaction == TransactionRequirement.None && idempotency == IdempotencyMechanism.Key)
        {
            throw new ArgumentException("A command that does not persist is idempotent naturally or by a client-supplied id, never by key.", nameof(idempotency));
        }

        return new(id, policy, OperationKind.Command, idempotency, transaction, rateLimit);
    }

    public static Operation Query(string id, string policy, string rateLimit = RateLimits.Default) =>
        new(id, policy, OperationKind.Query, IdempotencyMechanism.Natural, TransactionRequirement.None, rateLimit);
}
