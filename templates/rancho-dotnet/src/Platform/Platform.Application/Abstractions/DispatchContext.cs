using System.Security.Claims;

namespace Platform.Application.Abstractions;

/// <summary>
/// Who is acting, and the trace this work belongs to. The composition root builds it for each request, job, or reaction;
/// nothing reads the principal from an ambient source. It can't be built without a trace id.
/// </summary>
public sealed class DispatchContext
{
    public DispatchContext(ClaimsPrincipal principal, string principalId, string traceId, string? idempotencyKey = null)
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
        IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey;
    }

    public ClaimsPrincipal Principal { get; }

    /// <summary>The principal's stable id, as the audit trail and the idempotency record name it. Never a claim value with meaning.</summary>
    public string PrincipalId { get; }

    public string TraceId { get; }

    /// <summary>The client's Idempotency-Key, or an agent's key derived from its run and step.</summary>
    public string? IdempotencyKey { get; }
}
