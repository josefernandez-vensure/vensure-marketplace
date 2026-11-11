using Mediator;
using Microsoft.AspNetCore.Authorization;
using Platform.Application.Abstractions;

namespace Platform.Application.Behaviors;

/// <summary>
/// Evaluates the message's declared policy against the principal in the dispatch context, for every caller:
/// an endpoint, a job, an agent, a test. A transport filter may also enforce; it is never the only enforcement.
/// </summary>
public sealed class AuthorizationBehavior<TMessage, TResponse>(IAuthorizationService authorization, DispatchContext context)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage, IDeclaresOperation
    where TResponse : IResult<TResponse>
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = DispatchTelemetry.Source.StartActivity("authorize");
        var decision = await authorization.AuthorizeAsync(context.Principal, TMessage.Operation.Policy);
        if (!decision.Succeeded)
        {
            return TResponse.Failure(PlatformProblems.Forbidden.With());
        }

        return await next(message, cancellationToken);
    }
}
