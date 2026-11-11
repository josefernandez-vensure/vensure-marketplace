namespace Platform.Infrastructure.Persistence;

/// <summary>An event waiting to be delivered, written in the same transaction as the change that raised it.</summary>
internal sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public required string Type { get; set; }

    public required string Payload { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>The writing transaction's W3C traceparent; delivery restores it.</summary>
    public required string TraceParent { get; set; }

    public string? TraceState { get; set; }

    public required string InitiatingPrincipalId { get; set; }

    /// <summary>The id of the outbox message whose reaction wrote this one, if any.</summary>
    public Guid? CausationId { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }
}

/// <summary>The first attempt under an idempotency key, and what it answered.</summary>
internal sealed class IdempotencyRecord
{
    public long Id { get; set; }

    public required string PrincipalId { get; set; }

    public required string OperationId { get; set; }

    public required string Key { get; set; }

    public required string Fingerprint { get; set; }

    public bool Succeeded { get; set; }

    public string? ValueJson { get; set; }

    public string? ProblemType { get; set; }

    public int? ProblemStatus { get; set; }

    public string? ProblemTitle { get; set; }

    public bool? ProblemUserFacing { get; set; }

    public string? ProblemDetail { get; set; }

    public string? ProblemErrorsJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>One line of the audit trail: ids, never values. Append-only.</summary>
internal sealed class AuditRecord
{
    public long Id { get; set; }

    public DateTimeOffset At { get; set; }

    public required string PrincipalId { get; set; }

    public required string OperationId { get; set; }

    public required string Outcome { get; set; }

    public string? ProblemType { get; set; }

    public required string RecordIds { get; set; }

    public required string TraceId { get; set; }
}
