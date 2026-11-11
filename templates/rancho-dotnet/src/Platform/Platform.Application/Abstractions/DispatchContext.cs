using System.Security.Claims;

namespace Platform.Application.Abstractions;

/// <summary>
/// Who is acting, and the trace this work belongs to. The composition root builds it for each request, job, or reaction;
/// nothing reads the principal from an ambient source. It can't be built without a trace id.
/// </summary>
public sealed class DispatchContext
{
    public DispatchContext(ClaimsPrincipal principal, string traceId)
    {
        ArgumentNullException.ThrowIfNull(principal);
        if (traceId is not { Length: 32 } || !traceId.All(char.IsAsciiHexDigitLower))
        {
            throw new ArgumentException("The trace id is the 32-hex-digit W3C trace-id.", nameof(traceId));
        }

        Principal = principal;
        TraceId = traceId;
    }

    public ClaimsPrincipal Principal { get; }

    public string TraceId { get; }
}
