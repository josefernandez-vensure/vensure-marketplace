using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Logging;
using Platform.Application.Abstractions;
using SharedKernel;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

/// <summary>Every value that leaves the process carries a data class; unclassified is Restricted, and refused.</summary>
public sealed class ClassificationTests
{
    private static IEnumerable<Type> DomainEvents => TypesOf(Kinds.Domain).Where(t => !t.IsAbstract && typeof(IDomainEvent).IsAssignableFrom(t));

    private static IEnumerable<Type> IntegrationEvents => TypesOf(Kinds.Contracts).Where(t => !t.IsAbstract && typeof(IIntegrationEvent).IsAssignableFrom(t));

    /// <summary>What the outbox carries to other modules: integration events, the module's domain events, and published command messages.</summary>
    private static IEnumerable<Type> OutboxPayloads =>
        IntegrationEvents.Concat(DomainEvents).Concat(TypesOf(Kinds.Contracts).Where(t => DomainTests.IsMessage(t) || typeof(ICommandMessage).IsAssignableFrom(t)));

    /// <summary>The published surface: every type a module's Contracts declares.</summary>
    private static IEnumerable<Type> Published =>
        TypesOf(Kinds.Contracts).Where(t => !t.IsInterface && !(t.IsAbstract && t.IsSealed) && !typeof(Attribute).IsAssignableFrom(t));

    private static IEnumerable<Type> QueryResponses =>
        TypesOf(Kinds.Application, Kinds.Contracts).Where(DomainTests.IsMessage)
            .SelectMany(m => m.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>)))
            .Select(i => i.GetGenericArguments()[0])
            .Distinct();

    private static IEnumerable<MethodInfo> LoggingMethods =>
        Assemblies.SelectMany(a => a.Assembly.GetTypes())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(m => m.IsDefined(typeof(LoggerMessageAttribute)));

    private static IEnumerable<Type> ClassifiedAs(params DataClass[] classes) =>
        TypesOf(Kinds.All).Where(t => DataClassResolver.DeclaredBy(t) is { } c && classes.Contains(c));

    [Fact]
    public void Class_attributes_are_defined_only_in_the_shared_kernel()
    {
        TypesOf(Kinds.All)
            .Where(t => typeof(DataClassificationAttribute).IsAssignableFrom(t) || typeof(IClassOf).IsAssignableFrom(t))
            .Where(t => Owner(t)!.Kind != Kinds.SharedKernel)
            .Select(t => $"{t.FullName} defines a class outside SharedKernel")
            .ShouldHaveNoViolations("The data classes are SharedKernel's attributes: Public, Internal, Confidential, Restricted, and ClassOf<T>.");
    }

    [Fact]
    public void A_contracts_twin_declares_the_class_of_its_domain_type()
    {
        var domain = TypesOf(Kinds.Domain).ToLookup(t => (Owner(t)!.Module, t.Name));

        TypesOf(Kinds.Contracts)
            .SelectMany(c => domain[(Owner(c)!.Module, c.Name)].Select(d => (Contracts: c, Domain: d)))
            .Where(p => DataClassResolver.DeclaredBy(p.Domain) is not null || DataClassResolver.DeclaredBy(p.Contracts) is not null)
            .Where(p => DataClassResolver.DeclaredBy(p.Domain) != DataClassResolver.DeclaredBy(p.Contracts))
            .Select(p => $"{p.Contracts.FullName} is {Name(DataClassResolver.DeclaredBy(p.Contracts))}, but {p.Domain.FullName} is {Name(DataClassResolver.DeclaredBy(p.Domain))}")
            .ShouldHaveNoViolations("A value's class is declared on its Domain type and on its Contracts twin, and the two agree.");
    }

    [Fact]
    public void Everything_that_leaves_through_a_response_an_event_or_the_outbox_resolves_to_a_class()
    {
        Published.Concat(DomainEvents).Concat(QueryResponses).Distinct()
            .SelectMany(t => DataClassResolver.Resolve(t).Unresolved)
            .ShouldHaveNoViolations("Every value reachable from an egress boundary resolves to a class; unclassified is Restricted, and can't leave. "
                + "Declare the class on the value's type, or on a member whose type carries none: [Public], [Internal], [Confidential], [Restricted] or [ClassOf<T>].");
    }

    [Fact]
    public void A_member_declares_a_class_only_where_its_type_carries_none()
    {
        var violations = new List<string>();
        foreach (var type in TypesOf(Kinds.All).Where(t => DataClassResolver.DeclaredBy(t) is null))
        {
            foreach (var member in DataClassResolver.SerializedMembers(type))
            {
                var memberType = member is PropertyInfo p ? p.PropertyType : ((FieldInfo)member).FieldType;
                var valueType = DataClassResolver.ElementOf(memberType) ?? memberType;
                if (DataClassResolver.Declares(member) && DataClassResolver.DeclaredBy(valueType) is { } inherited)
                {
                    violations.Add($"{type.Name}.{member.Name} redeclares the class its type {valueType.Name} carries ({Name(inherited)})");
                }

                if (DataClassResolver.ClassOf(member) is { } source && DataClassResolver.DeclaredBy(source) is null)
                {
                    violations.Add($"{type.Name}.{member.Name} takes its class from {source.Name}, which declares none");
                }
            }
        }

        violations.ShouldHaveNoViolations("A class is declared once, on the value's type; a member that carries the value inherits it, and ClassOf<T> names a type that declares one.");
    }

    [Fact]
    public void Every_logging_parameter_declares_the_class_of_its_value()
    {
        var violations = new List<string>();
        foreach (var method in LoggingMethods)
        {
            foreach (var parameter in method.GetParameters().Where(p => !IsLoggingPlumbing(p.ParameterType)))
            {
                var where = $"{method.DeclaringType!.Name}.{method.Name}({parameter.Name})";
                var declared = parameter.GetCustomAttributes<DataClassificationAttribute>(false).Select(a => DataClasses.ToDataClass(a.Classification)).FirstOrDefault();
                if (declared is null)
                {
                    violations.Add($"{where} declares no class with [Public], [Internal], [Confidential] or [Restricted]; the redactor reads only those");
                }
                else if (DataClassResolver.DeclaredBy(parameter.ParameterType) is { } carried && carried != declared)
                {
                    violations.Add($"{where} is declared {Name(declared)}, but its type {parameter.ParameterType.Name} is {Name(carried)}");
                }
            }
        }

        violations.ShouldHaveNoViolations("Every logging-method parameter but the logger, level and exception declares its class, and it agrees with its type's.");
    }

    [Fact]
    public void Logging_methods_are_generated_by_the_redacting_generator()
    {
        LoggingMethods
            .Select(m => (Method: m, Tool: m.GetCustomAttribute<GeneratedCodeAttribute>()?.Tool))
            .Where(x => x.Tool != "Microsoft.Gen.Logging")
            .Select(x => $"{x.Method.DeclaringType!.FullName}.{x.Method.Name} is generated by {x.Tool ?? "nothing"}; reference Microsoft.Extensions.Telemetry.Abstractions in {Owner(x.Method.DeclaringType!)?.Name}")
            .ShouldHaveNoViolations("A project that declares a logging method references Microsoft.Extensions.Telemetry.Abstractions, whose generator redacts by class.");
    }

    [Fact]
    public void A_confidential_or_restricted_type_converts_implicitly_to_no_string()
    {
        ClassifiedAs(DataClass.Confidential, DataClass.Restricted)
            .Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static).Any(m => m.Name == "op_Implicit" && m.ReturnType == typeof(string)))
            .Select(t => $"{t.FullName} converts implicitly to string")
            .ShouldHaveNoViolations("A Confidential or Restricted value never becomes a string without someone asking for it.");
    }

    [Fact]
    public void A_confidential_or_restricted_type_redacts_in_ToString()
    {
        var violations = new List<string>();
        foreach (var type in ClassifiedAs(DataClass.Confidential, DataClass.Restricted).Where(t => !t.IsAbstract && !t.ContainsGenericParameters))
        {
            var instance = RuntimeHelpers.GetUninitializedObject(type);
            var planted = new List<string>();
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object? value = field.FieldType == typeof(string) ? $"planted-{Guid.NewGuid():N}"
                    : field.FieldType == typeof(Guid) ? Guid.NewGuid()
                    : field.FieldType == typeof(int) ? 918273645
                    : field.FieldType == typeof(long) ? 918273645019283L
                    : field.FieldType == typeof(decimal) ? 918273.645m
                    : null;
                if (value is not null)
                {
                    field.SetValue(instance, value);
                    planted.Add(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)!);
                }
            }

            var text = instance.ToString() ?? string.Empty;
            violations.AddRange(planted.Where(v => text.Contains(v, StringComparison.Ordinal)).Select(v => $"{type.FullName}.ToString() shows its value"));
        }

        violations.ShouldHaveNoViolations("A Confidential or Restricted type redacts in ToString(); a record's generated ToString shows every member.");
    }

    [Fact]
    public void Events_and_outbox_payloads_carry_nothing_restricted()
    {
        OutboxPayloads.Distinct()
            .Where(t => DataClassResolver.Resolve(t).Class == DataClass.Restricted)
            .Select(t => $"{t.FullName} carries Restricted data")
            .ShouldHaveNoViolations("Integration events and outbox rows never carry Restricted data; reference it by id.");
    }

    private static bool IsLoggingPlumbing(Type type) =>
        typeof(ILogger).IsAssignableFrom(type) || type == typeof(LogLevel) || typeof(Exception).IsAssignableFrom(type);

    private static string Name(DataClass? dataClass) => dataClass?.ToString() ?? "unclassified";
}
