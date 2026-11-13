using Mediator;
using Platform.Application.Abstractions;

namespace Platform.Application.Behaviors;

/// <summary>
/// Versioned writes only: a write without a usable If-Match is refused with 428, and one whose tag can never match with 412,
/// before the handler runs. Whether a matching tag is stale is decided by the unit of work, against the aggregate as loaded.
/// </summary>
public sealed class PreconditionBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage, IDeclaresOperation, IVersionedCommand
    where TResponse : IResult<TResponse>
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = DispatchTelemetry.StartStep("precondition", GetType());
        return message.ExpectedVersion switch
        {
            { IsMissing: true } => TResponse.Failure(PlatformProblems.PreconditionRequired.With()),
            { Unmatchable: true } => TResponse.Failure(PlatformProblems.PreconditionFailed.With()),
            _ => await next(message, cancellationToken),
        };
    }
}
