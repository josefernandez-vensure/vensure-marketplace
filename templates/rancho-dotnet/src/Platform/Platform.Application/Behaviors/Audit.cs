using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Platform.Application.Abstractions;

namespace Platform.Application.Behaviors;

internal static class Audit
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> NamingProperties = new();

    /// <summary>
    /// The line for a message: the ids of the records it names, and, for a read, of the records it returned. Values never enter the trail.
    /// </summary>
    public static AuditEntry Entry(object message, Operation operation, DispatchContext context, TimeProvider time, AuditOutcome outcome, Problem? problem, object? returned = null) =>
        new(time.GetUtcNow(), context.PrincipalId, operation.Id, outcome, problem?.Type.Uri,
            [.. IdsIn(message, isItem: false).Concat(returned is null ? [] : IdsIn(returned, isItem: false)).Distinct()], context.TraceId,
            context.Causation?.InitiatingPrincipalId, context.Causation?.MessageId);

    /// <summary>
    /// The ids an object holds: its strongly-typed id properties, and those of each item of a collection it holds,
    /// such as the products a command over several aggregates names or a page's items.
    /// </summary>
    private static IEnumerable<Guid> IdsIn(object value, bool isItem)
    {
        foreach (var property in NamingPropertiesOf(value.GetType()))
        {
            switch (property.GetValue(value))
            {
                case null:
                    break;
                case var id when IsStronglyTypedId(property.PropertyType):
                    yield return GuidOf(id);
                    break;
                case IEnumerable items when !isItem:
                    foreach (var item in items.OfType<object>())
                    {
                        if (IsStronglyTypedId(item.GetType()))
                        {
                            yield return GuidOf(item);
                        }
                        else
                        {
                            foreach (var id in IdsIn(item, isItem: true))
                            {
                                yield return id;
                            }
                        }
                    }

                    break;
            }
        }
    }

    private static PropertyInfo[] NamingPropertiesOf(Type type) =>
        NamingProperties.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0)
            .Where(p => IsStronglyTypedId(p.PropertyType) || (p.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.PropertyType)))
            .ToArray());

    private static Guid GuidOf(object id) =>
        (Guid)id.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0].GetValue(id)!;

    private static bool IsStronglyTypedId(Type type) =>
        type.IsValueType
        && type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is [{ FieldType: var field }]
        && field == typeof(Guid);
}
