using System.Collections.Concurrent;
using System.Reflection;
using Platform.Application.Abstractions;

namespace Platform.Application.Behaviors;

internal static class Audit
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> IdProperties = new();

    public static AuditEntry Entry(object message, Operation operation, DispatchContext context, TimeProvider time, AuditOutcome outcome, Problem? problem) =>
        new(time.GetUtcNow(), context.PrincipalId, operation.Id, outcome, problem?.Type.Uri, RecordIds(message), context.TraceId,
            context.Causation?.InitiatingPrincipalId, context.Causation?.MessageId);

    /// <summary>The ids of the records a message names: its strongly-typed id properties. Values never enter the trail.</summary>
    private static List<Guid> RecordIds(object message) =>
        IdProperties.GetOrAdd(message.GetType(), type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => IsStronglyTypedId(p.PropertyType))
                .ToArray())
            .Select(p => p.GetValue(message))
            .Select(id => (Guid)id!.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0].GetValue(id)!)
            .ToList();

    private static bool IsStronglyTypedId(Type type) =>
        type.IsValueType
        && type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is [{ FieldType: var field }]
        && field == typeof(Guid);
}
