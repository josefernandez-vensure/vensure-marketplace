using System.Security.Claims;

namespace Platform.Application.Abstractions;

/// <summary>
/// Who is acting, which records they may see, the trace this work belongs to, and, for a reaction, what caused it. The composition root builds it for each request, job, or reaction;
/// nothing reads the principal from an ambient source. It can't be built without a trace id.
/// </summary>
public sealed class DispatchContext
{
    public DispatchContext(ClaimsPrincipal principal, string principalId, string traceId, TenantId? tenant, string? idempotencyKey = null, Causation? causation = null)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(principalId);
        if (traceId is not { Length: 32 } || !traceId.All(char.IsAsciiHexDigitLower))
        {
            throw new ArgumentException("The trace id is the 32-hex-digit W3C trace-id.", nameof(traceId));
        }

        Principal = principal;
        PrincipalId = principalId;
        TraceId = traceId;
        Tenant = tenant;
        IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey;
        Causation = causation;
    }

    public ClaimsPrincipal Principal { get; }

    /// <summary>The principal's stable id, as the audit trail and the idempotency record name it. Never a claim value with meaning.</summary>
    public string PrincipalId { get; }

    public string TraceId { get; }

    /// <summary>The records the principal may see: its tenant's. Null sees none. Every read and write of tenant data is scoped by it.</summary>
    public TenantId? Tenant { get; }

    /// <summary>The client's Idempotency-Key, an agent's key derived from its run and step, or the id of the message a reaction answers.</summary>
    public string? IdempotencyKey { get; }

    /// <summary>For a reaction: the message that caused it and the user it acts on behalf of. Null for any other caller.</summary>
    public Causation? Causation { get; }
}

/// <summary>Why a reaction runs: the outbox message that caused it, and the user whose action started the chain.</summary>
public sealed record Causation(Guid MessageId, string InitiatingPrincipalId);
