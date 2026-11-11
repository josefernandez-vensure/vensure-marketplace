using Mediator;
using Microsoft.Extensions.Logging;
using Platform.Application.Abstractions;

namespace Platform.Application.Behaviors;

/// <summary>First in the pipeline: one span and one log line per dispatch. Handlers don't log.</summary>
public sealed class LoggingBehavior<TMessage, TResponse>(ILogger<LoggingBehavior<TMessage, TResponse>> logger, TimeProvider time)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage, IDeclaresOperation
    where TResponse : IResult<TResponse>
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var operation = TMessage.Operation;
        using var activity = DispatchTelemetry.Source.StartActivity(operation.Id);
        activity?.SetTag("operation.kind", operation.Kind.ToString());

        var started = time.GetTimestamp();
        var result = await next(message, cancellationToken);
        var elapsed = time.GetElapsedTime(started).TotalMilliseconds;

        if (result.IsSuccess)
        {
            DispatchTelemetry.Succeeded(logger, operation.Id, elapsed);
        }
        else
        {
            activity?.SetTag("problem.type", result.Problem!.Type.Uri);
            DispatchTelemetry.Failed(logger, operation.Id, result.Problem!.Type.Uri, elapsed);
        }

        return result;
    }
}
