using System.Collections;
using System.Reflection;
using SharedKernel;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

public sealed class DomainTests
{
    private static readonly BindingFlags Declared = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private static IEnumerable<Type> AggregateRoots =>
        TypesOf(Kinds.Domain).Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

    [Fact]
    public void Aggregate_state_is_private()
    {
        AggregateRoots
            .SelectMany(root => HierarchyOf(root).SelectMany(t =>
                t.GetProperties(Declared)
                    .Where(p => p.SetMethod?.IsPublic == true)
                    .Select(p => $"{root.Name}.{p.Name} has a public setter")
                .Concat(t.GetFields(Declared).Select(f => $"{root.Name}.{f.Name} is a public field"))
                .Concat(t.GetProperties(Declared)
                    .Where(p => p.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                    .Where(p => !(p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>)))
                    .Select(p => $"{root.Name}.{p.Name} exposes {p.PropertyType.Name}; expose IReadOnlyCollection<T>"))))
            .ShouldHaveNoViolations("An aggregate's state is private; it changes only through methods named in domain vocabulary.");
    }

    [Fact]
    public void Domain_does_no_io()
    {
        string[] io = ["System.Net", "System.Data", "Microsoft.Data", "Microsoft.EntityFrameworkCore"];
        string[] files = ["System.IO.File", "System.IO.FileInfo", "System.IO.FileStream", "System.IO.Directory", "System.IO.DirectoryInfo", "System.IO.DriveInfo"];

        DependenciesOf(ArchTypesOf(Kinds.Domain, Kinds.SharedKernel))
            .Where(d => io.Any(ns => InNamespace(d.Target, ns)) || files.Contains(d.Target.FullName))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("The domain does no I/O: no network, database, or file system.");
    }

    [Fact]
    public void Domain_events_stay_in_their_module()
    {
        var events = TypesOf(Kinds.Domain).Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && !t.IsInterface).ToDictionary(t => t.FullName!, t => Owner(t)!.Module);

        DependenciesOf(Model.Types)
            .Where(d => events.ContainsKey(d.Target.FullName) && Owner(d.Origin)?.Module != events[d.Target.FullName])
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("Domain events reach their own module only; other modules hear integration events.");
    }

    [Fact]
    public void Ids_are_strongly_typed()
    {
        var messages = TypesOf(Kinds.Application, Kinds.Contracts).Where(IsMessage);

        TypesOf(Kinds.Domain).Concat(messages)
            .Where(t => !IsStronglyTypedId(t))
            .SelectMany(t => PublicSignatureTypes(t).Where(x => Unwrap(x.Type).Contains(typeof(Guid))).Select(x => $"{t.Name}.{x.Member} uses Guid"))
            .ShouldHaveNoViolations("Ids are strongly typed, one type per concept; no bare Guid in the domain or in a message.");
    }

    [Fact]
    public void Domain_types_carry_no_mapping_attributes()
    {
        TypesOf(Kinds.Domain)
            .SelectMany(t => new MemberInfo[] { t }.Concat(t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)))
            .SelectMany(m => m.GetCustomAttributes(false).Select(a => (Member: m, Attribute: a.GetType())))
            .Where(x => x.Attribute.Namespace?.StartsWith("System.ComponentModel.DataAnnotations", StringComparison.Ordinal) == true)
            .Select(x => $"{x.Member.DeclaringType?.Name ?? x.Member.Name}.{x.Member.Name} has [{x.Attribute.Name}]")
            .ShouldHaveNoViolations("Mapping lives in IEntityTypeConfiguration<T> classes; domain types carry no mapping attributes.");
    }

    [Fact]
    public void An_aggregate_holds_other_aggregates_by_id()
    {
        var roots = AggregateRoots.ToHashSet();

        roots
            .SelectMany(root => HierarchyOf(root).SelectMany(t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(p => (Name: p.Name, Type: p.PropertyType))
                .Concat(t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(f => (Name: f.Name, Type: f.FieldType))))
                .Where(m => Unwrap(m.Type).Any(roots.Contains))
                .Select(m => $"{root.Name}.{m.Name} holds an aggregate; hold its id"))
            .ShouldHaveNoViolations("An aggregate holds another aggregate's id, never a reference to it.");
    }

    [Fact]
    public void Repositories_expose_no_IQueryable()
    {
        TypesOf(Kinds.Domain)
            .Where(t => t.IsInterface && Implements(t, typeof(IRepository<,>)))
            .SelectMany(t => t.GetMethods().Concat(t.GetInterfaces().SelectMany(i => i.GetMethods()))
                .SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType).Select(type => (t, m, type))))
            .Where(x => Unwrap(x.type).Any(u => typeof(IQueryable).IsAssignableFrom(u)))
            .Select(x => $"{x.t.Name}.{x.m.Name} exposes {x.type.Name}")
            .ShouldHaveNoViolations("A repository loads and saves whole aggregates and exposes no IQueryable.");
    }

    [Fact]
    public void Domain_outcomes_are_enums()
    {
        TypesOf(Kinds.Domain)
            .Where(t => !t.IsEnum && (t.Name.EndsWith("Outcome", StringComparison.Ordinal) || t.Name.EndsWith("Result", StringComparison.Ordinal)))
            .Select(t => $"{t.FullName} is an outcome that isn't an enum")
            .ShouldHaveNoViolations("A business rejection is an outcome enum, so a translation with no discard arm fails the build when an outcome is added (CS8509).");
    }

    internal static bool IsMessage(Type type) =>
        !type.IsAbstract && type.GetInterfaces().Any(i => i.FullName == "Mediator.IMessage");

    private static IEnumerable<Type> HierarchyOf(Type type)
    {
        for (var t = type; t is not null && t != typeof(object); t = t.BaseType)
        {
            yield return t.IsGenericType ? t.GetGenericTypeDefinition() : t;
        }
    }

    private static IEnumerable<(string Member, Type Type)> PublicSignatureTypes(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        foreach (var p in type.GetProperties(flags))
        {
            yield return (p.Name, p.PropertyType);
        }

        foreach (var f in type.GetFields(flags))
        {
            yield return (f.Name, f.FieldType);
        }

        foreach (var m in type.GetMethods(flags).Where(m => !m.IsSpecialName && !m.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute))))
        {
            yield return (m.Name, m.ReturnType);
            foreach (var p in m.GetParameters())
            {
                yield return ($"{m.Name}({p.Name})", p.ParameterType);
            }
        }

        foreach (var c in type.GetConstructors())
        {
            foreach (var p in c.GetParameters())
            {
                yield return ($"ctor({p.Name})", p.ParameterType);
            }
        }
    }
}
