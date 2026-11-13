using System.Reflection;
using System.Runtime.CompilerServices;
using SharedKernel;
using Shouldly;

namespace Domain.Tests;

/// <summary>
/// The domain's behaviour is tested member by member. A test is named <c>&lt;Member&gt;_&lt;Rule&gt;_When&lt;Condition&gt;</c>, in
/// <c>&lt;Type&gt;Tests</c>, with <c>New</c> for a constructor; a method that returns an outcome has a test named
/// <c>&lt;Method&gt;_Returns&lt;Outcome&gt;_…</c> for each of its outcomes.
/// </summary>
public sealed class EveryRuleHasATest
{
    private static readonly string[] RecordMembers = ["Equals", "GetHashCode", "ToString", "Deconstruct", "PrintMembers"];

    [Fact]
    public void Every_domain_method_constructor_and_outcome_has_its_test()
    {
        var tests = typeof(EveryRuleHasATest).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods().Where(m => m.IsDefined(typeof(FactAttribute), true)).Select(m => (Class: t.Name, m.Name)))
            .ToLookup(t => t.Class, t => t.Name);
        var missing = new List<string>();

        foreach (var type in DomainTypes())
        {
            var named = tests[$"{type.Name}Tests"].ToList();
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         .Where(m => !m.IsSpecialName && !RecordMembers.Contains(m.Name) && !m.Name.StartsWith('<')))
            {
                if (!named.Any(n => n.StartsWith(method.Name + "_", StringComparison.Ordinal)))
                {
                    missing.Add($"{type.Name}Tests has no test named {method.Name}_…");
                }

                if (method.ReturnType.IsEnum && method.ReturnType.Name.EndsWith("Outcome", StringComparison.Ordinal))
                {
                    missing.AddRange(Enum.GetNames(method.ReturnType)
                        .Where(outcome => !named.Any(n => n.StartsWith($"{method.Name}_Returns{outcome}_", StringComparison.Ordinal)))
                        .Select(outcome => $"{type.Name}Tests has no test named {method.Name}_Returns{outcome}_…"));
                }
            }

            if (type.GetConstructors().Any(c => c.GetParameters().Length > 0 && !IsPrimaryConstructorOfRecord(type))
                && !named.Any(n => n.StartsWith("New_", StringComparison.Ordinal)))
            {
                missing.Add($"{type.Name}Tests has no test named New_…");
            }
        }

        missing.ShouldBeEmpty("Every invariant and every outcome of the domain has a test.");
    }

    /// <summary>Aggregates and value objects of every module's Domain: not ids, events, enums or interfaces, which carry no rule.</summary>
    private static IEnumerable<Type> DomainTypes() =>
        Directory.GetFiles(AppContext.BaseDirectory, "*.Domain.dll")
            .Select(Assembly.LoadFrom)
            .Where(a => a.GetCustomAttributes<AssemblyMetadataAttribute>().Any(m => m.Key == "ProjectKind" && m.Value == "Domain"))
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && !t.IsDefined(typeof(CompilerGeneratedAttribute)))
            .Where(t => !typeof(IDomainEvent).IsAssignableFrom(t) && !IsStronglyTypedId(t));

    private static bool IsStronglyTypedId(Type type) =>
        type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is [{ FieldType: var field }] && field == typeof(Guid);

    /// <summary>A positional record's constructor only assigns; a constructor that validates is written out.</summary>
    private static bool IsPrimaryConstructorOfRecord(Type type) =>
        type.GetMethod("<Clone>$") is not null && type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance) is not null
        && type.GetConstructors().All(c => c.GetParameters().All(p => type.GetProperty(p.Name!, BindingFlags.Public | BindingFlags.Instance)?.GetMethod?.IsDefined(typeof(CompilerGeneratedAttribute)) == true));
}
