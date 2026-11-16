using System.Reflection;
using Mediator;
using Platform.Application.Abstractions;
using SharedKernel;

namespace Platform.Application.Behaviors;

/// <summary>
/// Queries only: a read whose response carries Confidential or Restricted data, or data whose class can't be resolved, is recorded in the audit
/// trail with the ids it named and the ids it returned. An export is a query too, so it is audited the same way. A denial is the
/// authorization behavior's record.
/// </summary>
public sealed class ReadAuditBehavior<TMessage, TResponse>(IUnitOfWorkFactory units, DispatchContext context, TimeProvider time)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage, IBaseQuery, IDeclaresOperation
    where TResponse : IResult<TResponse>
{
    private static readonly PropertyInfo? ValueProperty = typeof(TResponse).GetProperty(nameof(Result<int>.Value));

    /// <summary>Whether this query's response is audited: decided once, from the class of what it returns.</summary>
    public static bool Audited { get; } = ValueProperty is not null
        && DataClassResolver.Resolve(ValueProperty.PropertyType) is not { IsResolved: true, Class: < DataClass.Confidential };

    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = DispatchTelemetry.StartStep("audit-read", GetType());
        var result = await next(message, cancellationToken);
        if (!Audited)
        {
            return result;
        }

        var returned = result.IsSuccess ? ValueProperty?.GetValue(result) : null;
        var unit = units.For(typeof(TMessage));
        unit.StageAudit(Audit.Entry(message, TMessage.Operation, context, time, result.IsSuccess ? AuditOutcome.Succeeded : AuditOutcome.Failed, result.Problem, returned));
        await unit.Commit(cancellationToken);
        return result;
    }
}
