using Mediator;
using Platform.Application.Abstractions;

namespace Platform.Application.Behaviors;

/// <summary>
/// Paged queries only: a page size outside 1..MaxPageSize, or a cursor this query didn't issue, is refused before the handler runs.
/// A request above the maximum is rejected, never clamped.
/// </summary>
public sealed class PagingBehavior<TMessage, TResponse>(ICursorCodec cursors) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage, IDeclaresOperation, IPagedQuery
    where TResponse : IResult<TResponse>
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = DispatchTelemetry.StartStep("page", GetType());
        var errors = new Dictionary<string, string[]>();
        if (message.PageSize < 1 || message.PageSize > TMessage.MaxPageSize)
        {
            errors["pageSize"] = [$"The page size must be between 1 and {TMessage.MaxPageSize}."];
        }

        if (message.Cursor is { } token && cursors.Open(token, TMessage.Operation.Id) is null)
        {
            errors["cursor"] = ["The cursor is not one this query issued. Start again without it."];
        }

        return errors.Count == 0
            ? await next(message, cancellationToken)
            : TResponse.Failure(new Problem(PlatformProblems.Validation, Errors: errors));
    }
}
