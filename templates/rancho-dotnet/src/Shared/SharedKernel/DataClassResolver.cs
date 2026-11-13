using System.Collections;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Compliance.Classification;

namespace SharedKernel;

/// <summary>A value's class, or the members that left it unresolved. Unresolved is Restricted, and every egress check refuses it.</summary>
public sealed record DataClassResolution(DataClass? Class, IReadOnlyList<string> Unresolved)
{
    public bool IsResolved => Class is not null && Unresolved.Count == 0;
}

/// <summary>
/// Resolves the class of anything that crosses an egress boundary. A type that declares a class carries it, members included.
/// A member or parameter that declares one, directly or through <see cref="ClassOfAttribute{T}"/>, carries that. A collection
/// carries its element's class, and any other type the most protected class of what it serializes. Anything else is unresolved.
/// </summary>
public static class DataClassResolver
{
    private static readonly HashSet<Type> Scalars =
    [
        typeof(string), typeof(decimal), typeof(Guid), typeof(DateTime), typeof(DateTimeOffset), typeof(DateOnly), typeof(TimeOnly),
        typeof(TimeSpan), typeof(Uri), typeof(Version), typeof(object),
    ];

    /// <summary>The class a type declares, if any.</summary>
    public static DataClass? DeclaredBy(Type type) => FromAttributes(Nullable.GetUnderlyingType(type) ?? type);

    /// <summary>
    /// The class a property, field or parameter declares, directly or through <see cref="ClassOfAttribute{T}"/>. A positional record's
    /// property finds what was declared on its constructor parameter, where the compiler puts it.
    /// </summary>
    public static DataClass? DeclaredOn(ICustomAttributeProvider member) =>
        FromAttributes(member) ?? (member is PropertyInfo property && PrimaryParameter(property) is { } parameter ? FromAttributes(parameter) : null);

    /// <summary>Whether the member declares a class itself, rather than inheriting one from its type.</summary>
    public static bool Declares(ICustomAttributeProvider member) => DeclaredOn(member) is not null || ClassOf(member) is not null;

    /// <summary>The type a member names with <see cref="ClassOfAttribute{T}"/>, if it does.</summary>
    public static Type? ClassOf(ICustomAttributeProvider member) =>
        Attributes(member).OfType<IClassOf>().FirstOrDefault()?.Source
        ?? (member is PropertyInfo property && PrimaryParameter(property) is { } parameter ? Attributes(parameter).OfType<IClassOf>().FirstOrDefault()?.Source : null);

    public static DataClassResolution Resolve(Type type) => Resolve(type, type.Name, []);

    /// <summary>A member of a type that declares a class is that class: the type's state is the classified value.</summary>
    public static DataClassResolution Resolve(PropertyInfo property) =>
        property.DeclaringType is { } owner && DeclaredBy(owner) is { } ownerClass
            ? new(ownerClass, [])
            : Resolve(property, property.PropertyType, $"{property.DeclaringType?.Name}.{property.Name}", []);

    public static DataClassResolution Resolve(FieldInfo field) =>
        field.DeclaringType is { } owner && DeclaredBy(owner) is { } ownerClass
            ? new(ownerClass, [])
            : Resolve(field, field.FieldType, $"{field.DeclaringType?.Name}.{field.Name}", []);

    public static DataClassResolution Resolve(ParameterInfo parameter) =>
        Resolve(parameter, parameter.ParameterType, $"{parameter.Member.DeclaringType?.Name}.{parameter.Member.Name}({parameter.Name})", []);

    /// <summary>The members serialization writes: public readable properties and <c>[JsonInclude]</c> fields, unless ignored.</summary>
    public static IEnumerable<MemberInfo> SerializedMembers(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetMethod?.IsPublic == true && p.GetIndexParameters().Length == 0)
            .Cast<MemberInfo>()
            .Concat(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.IsDefined(typeof(JsonIncludeAttribute))))
            .Where(m => m.GetCustomAttribute<JsonIgnoreAttribute>() is not { Condition: JsonIgnoreCondition.Always });

    /// <summary>The element type of a collection (a dictionary's value type), or null for anything else.</summary>
    public static Type? ElementOf(Type type)
    {
        if (type == typeof(string))
        {
            return null;
        }

        if (type.IsArray)
        {
            return type.GetElementType();
        }

        var interfaces = type.IsInterface ? type.GetInterfaces().Append(type) : type.GetInterfaces();
        var dictionary = interfaces.FirstOrDefault(i => i.IsGenericType
            && (i.GetGenericTypeDefinition() == typeof(IDictionary<,>) || i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)));
        if (dictionary is not null)
        {
            return dictionary.GetGenericArguments()[1];
        }

        return interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))?.GetGenericArguments()[0]
            ?? (typeof(IEnumerable).IsAssignableFrom(type) ? typeof(object) : null);
    }

    public static DataClass MostProtected(DataClass a, DataClass b) => a > b ? a : b;

    private static DataClassResolution Resolve(ICustomAttributeProvider member, Type type, string path, HashSet<Type> visiting)
    {
        if (DeclaredOn(member) is { } declared)
        {
            return new(declared, []);
        }

        if (ClassOf(member) is { } source)
        {
            return DeclaredBy(source) is { } named ? new(named, []) : new(null, [$"{path} names {source.Name}, which declares no class"]);
        }

        return Resolve(type, path, visiting);
    }

    private static DataClassResolution Resolve(Type type, string path, HashSet<Type> visiting)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (DeclaredBy(type) is { } declared)
        {
            return new(declared, []);
        }

        if (ElementOf(type) is { } element)
        {
            return Resolve(element, path, visiting);
        }

        if (type.IsPrimitive || type.IsEnum || Scalars.Contains(type) || IsFramework(type))
        {
            return new(null, [$"{path} is {type.Name}, which declares no class"]);
        }

        if (!visiting.Add(type))
        {
            return new(DataClass.Public, []);
        }

        var result = DataClass.Public;
        var unresolved = new List<string>();
        var members = SerializedMembers(type).Select(m => m switch
            {
                PropertyInfo p => Resolve(p, p.PropertyType, $"{path}.{p.Name}", visiting),
                FieldInfo f => Resolve(f, f.FieldType, $"{path}.{f.Name}", visiting),
                _ => throw new InvalidOperationException(),
            })
            .Concat(type.GetCustomAttributes<JsonDerivedTypeAttribute>(false).Select(d => Resolve(d.DerivedType, $"{path}<{d.DerivedType.Name}>", visiting)));
        foreach (var member in members)
        {
            unresolved.AddRange(member.Unresolved);
            if (member.Class is { } memberClass)
            {
                result = MostProtected(result, memberClass);
            }
        }

        visiting.Remove(type);
        return unresolved.Count == 0 ? new(result, []) : new(null, unresolved);
    }

    private static DataClass? FromAttributes(ICustomAttributeProvider provider) =>
        Attributes(provider).OfType<DataClassificationAttribute>().Select(a => DataClasses.ToDataClass(a.Classification)).FirstOrDefault(c => c is not null);

    private static object[] Attributes(ICustomAttributeProvider provider) => provider.GetCustomAttributes(false);

    private static ParameterInfo? PrimaryParameter(PropertyInfo property) =>
        property.DeclaringType?.GetConstructors()
            .SelectMany(c => c.GetParameters())
            .FirstOrDefault(p => p.Name == property.Name && p.ParameterType == property.PropertyType);

    private static bool IsFramework(Type type) =>
        type.Namespace is { } ns && (ns == "System" || ns.StartsWith("System.", StringComparison.Ordinal) || ns.StartsWith("Microsoft.", StringComparison.Ordinal));
}
