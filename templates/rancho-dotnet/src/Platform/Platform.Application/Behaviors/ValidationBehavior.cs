using FluentValidation;
using Mediator;
using Platform.Application.Abstractions;

namespace Platform.Application.Behaviors;

/// <summary>Runs the message's validators. A failure short-circuits: the handler never sees an invalid message.</summary>
public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage, IDeclaresOperation
    where TResponse : IResult<TResponse>
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = DispatchTelemetry.StartStep("validate", GetType());
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var validator in validators)
        {
            var outcome = await validator.ValidateAsync(message, cancellationToken);
            failures.AddRange(outcome.Errors);
        }

        if (failures.Count == 0)
        {
            return await next(message, cancellationToken);
        }

        var errors = failures
            .GroupBy(f => ToRequestPath(f.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
        return TResponse.Failure(new Problem(PlatformProblems.Validation, Errors: errors));
    }

    /// <summary><c>Address.Lines[0]</c> becomes <c>address.lines[0]</c>: JSON property names as sent.</summary>
    private static string ToRequestPath(string propertyName) =>
        string.Join('.', propertyName.Split('.').Select(segment => segment.Length == 0 ? segment : char.ToLowerInvariant(segment[0]) + segment[1..]));
}
