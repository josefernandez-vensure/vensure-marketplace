using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Platform.Application.Abstractions;

namespace Platform.Application.Behaviors;

internal static class Audit
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> IdProperties = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> CollectionProperties = new();

    /// <summary>
    /// The line for a message: the ids of the records it names, and, for a read, of the records it returned. Values never enter the trail.
    /// </summary>
    public static AuditEntry Entry(object message, Operation operation, DispatchContext context, TimeProvider time, AuditOutcome outcome, Problem? problem, object? returned = null) =>
        new(time.GetUtcNow(), context.PrincipalId, operation.Id, outcome, problem?.Type.Uri, [.. RecordIds(message).Concat(ReturnedIds(returned)).Distinct()], context.TraceId,
            context.Causation?.InitiatingPrincipalId, context.Causation?.MessageId);

    /// <summary>The strongly-typed ids an object holds in its own properties.</summary>
    private static IEnumerable<Guid> RecordIds(object value) =>
        IdProperties.GetOrAdd(value.GetType(), type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => IsStronglyTypedId(p.PropertyType) && p.GetIndexParameters().Length == 0)
                .ToArray())
            .Select(p => p.GetValue(value))
            .Select(id => (Guid)id!.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0].GetValue(id)!);

    /// <summary>What a read returned: the ids in the response, and in each item of a collection it holds, such as a page's items.</summary>
    private static IEnumerable<Guid> ReturnedIds(object? returned)
    {
        if (returned is null)
        {
            return [];
        }

        var items = CollectionProperties.GetOrAdd(returned.GetType(), type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.PropertyType) && p.GetIndexParameters().Length == 0)
                .ToArray())
            .SelectMany(p => (p.GetValue(returned) as IEnumerable)?.Cast<object>() ?? [])
            .Where(item => item is not null);
        return RecordIds(returned).Concat(items.SelectMany(RecordIds));
    }

    private static bool IsStronglyTypedId(Type type) =>
        type.IsValueType
        && type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is [{ FieldType: var field }]
        && field == typeof(Guid);
}
