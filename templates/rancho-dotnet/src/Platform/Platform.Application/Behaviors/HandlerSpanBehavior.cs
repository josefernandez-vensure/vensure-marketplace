using Mediator;
using Platform.Application.Abstractions;

namespace Platform.Application.Behaviors;

/// <summary>Last in the pipeline: the span around the handler alone, so its time is separable from every step before it.</summary>
public sealed class HandlerSpanBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage, IDeclaresOperation
    where TResponse : IResult<TResponse>
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = DispatchTelemetry.StartStep("handle", GetType());
        return await next(message, cancellationToken);
    }
}
