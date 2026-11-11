using Api.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Platform.Application.Abstractions;

namespace Api.Http;

/// <summary>
/// A request the transport refuses never reaches the pipeline, so its denial is recorded here.
/// Every failed authorization is in the audit trail, whichever layer refused it.
/// </summary>
public sealed class AuditingAuthorizationResultHandler(TimeProvider time) : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden && context.GetEndpoint()?.Metadata.GetMetadata<DiscoveredEndpoint>() is { } endpoint)
        {
            var dispatch = context.RequestServices.GetRequiredService<DispatchContext>();
            var unit = context.RequestServices.GetRequiredService<IUnitOfWorkFactory>().For(endpoint.MessageType);
            unit.StageAudit(new AuditEntry(
                time.GetUtcNow(),
                dispatch.PrincipalId,
                endpoint.Operation.Id,
                AuditOutcome.Denied,
                PlatformProblems.Forbidden.Uri,
                [],
                dispatch.TraceId));
            await unit.Commit(context.RequestAborted);
        }

        await _default.HandleAsync(next, context, policy, authorizeResult);
    }
}
