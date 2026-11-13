using System.Reflection;
using System.Runtime.CompilerServices;
using ArchModel = ArchUnitNET.Domain.Architecture;
using IType = ArchUnitNET.Domain.IType;
using MethodCallDependency = ArchUnitNET.Domain.Dependencies.MethodCallDependency;

namespace Architecture.Tests;

/// <summary>
/// Every production assembly, found by the ProjectKind each project declares (build/ProjectKinds.targets).
/// Reflection answers questions about shape; ArchUnitNET answers questions about dependencies, method bodies included.
/// </summary>
internal static class Solution
{
    private static readonly Lazy<Loaded> Instance = new(Load);

    public static IReadOnlyList<ProductionAssembly> Assemblies => Instance.Value.Assemblies;

    public static ArchModel Model => Instance.Value.Model;

    public static IEnumerable<ProductionAssembly> OfKind(params string[] kinds) =>
        Assemblies.Where(a => kinds.Contains(a.Kind));

    /// <summary>Types declared in the given kinds, nested and compiler-generated ones excluded.</summary>
    public static IEnumerable<Type> TypesOf(params string[] kinds) =>
        OfKind(kinds).SelectMany(a => a.Assembly.GetTypes()).Where(t => !t.IsNested && !IsGenerated(t));

    public static IEnumerable<IType> ArchTypesOf(params string[] kinds)
    {
        var names = OfKind(kinds).Select(a => a.Name).ToHashSet();
        return Model.Types.Where(t => names.Contains(AssemblyName(t)));
    }

    /// <summary>The type and every type nested in it, such as an async state machine or a lambda closure.</summary>
    public static IEnumerable<IType> WithNested(Type type) =>
        Model.Types.Where(t =>
            t.FullName == type.FullName
            || t.FullName.StartsWith(type.FullName + "+", StringComparison.Ordinal)
            || t.FullName.StartsWith(type.FullName + "/", StringComparison.Ordinal));

    public static ProductionAssembly? Owner(IType type) =>
        Assemblies.FirstOrDefault(a => a.Name == AssemblyName(type));

    public static ProductionAssembly? Owner(Type type) =>
        Assemblies.FirstOrDefault(a => a.Assembly == type.Assembly);

    public static IEnumerable<(IType Origin, IType Target)> DependenciesOf(IEnumerable<IType> types) =>
        types.SelectMany(t => t.Dependencies.Select(d => (t, d.Target)));

    public static IEnumerable<(IType Origin, string Method)> CallsOf(IEnumerable<IType> types) =>
        types.SelectMany(t => t.Dependencies.OfType<MethodCallDependency>().Select(d => (t, d.TargetMember.Name)));

    public static bool InNamespace(IType type, string prefix) =>
        type.FullName == prefix || type.FullName.StartsWith(prefix + ".", StringComparison.Ordinal);

    public static bool IsGenerated(Type type) =>
        type.IsDefined(typeof(CompilerGeneratedAttribute), false)
        || type.IsDefined(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute), false)
        || type.Name.StartsWith('<');

    public static bool Implements(Type type, Type openOrClosedInterface) =>
        type.GetInterfaces().Any(i => i == openOrClosedInterface || (i.IsGenericType && i.GetGenericTypeDefinition() == openOrClosedInterface));

    /// <summary>A static class whose public methods register services: <c>Add&lt;Module&gt;Module</c> and its kind.</summary>
    public static bool IsRegistrationEntryPoint(Type type) =>
        type is { IsAbstract: true, IsSealed: true }
        && type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Any(m => m.GetParameters().FirstOrDefault()?.ParameterType.FullName == "Microsoft.Extensions.DependencyInjection.IServiceCollection");

    /// <summary>A struct or record whose only instance state is one Guid.</summary>
    public static bool IsStronglyTypedId(Type type)
    {
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return fields.Length == 1 && fields[0].FieldType == typeof(Guid);
    }

    /// <summary>The type and every generic argument and element type inside it.</summary>
    public static IEnumerable<Type> Unwrap(Type type)
    {
        yield return type;
        if (type.HasElementType)
        {
            foreach (var t in Unwrap(type.GetElementType()!))
            {
                yield return t;
            }
        }

        if (type.IsGenericType)
        {
            foreach (var t in type.GetGenericArguments().SelectMany(Unwrap))
            {
                yield return t;
            }
        }
    }

    public static void ShouldHaveNoViolations(this IEnumerable<string> violations, string rule)
    {
        var list = violations.Distinct().Order().ToList();
        if (list.Count > 0)
        {
            Assert.Fail($"{rule}\n  - {string.Join("\n  - ", list)}");
        }
    }

    private static string AssemblyName(IType type) => type.Assembly.FullName.Split(',')[0];

    private static Loaded Load()
    {
        var assemblies = new List<ProductionAssembly>();
        foreach (var path in Directory.GetFiles(AppContext.BaseDirectory, "*.dll"))
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(path);
            }
            catch (BadImageFormatException)
            {
                continue;
            }

            var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToDictionary(a => a.Key, a => a.Value);
            if (metadata.TryGetValue("ProjectKind", out var kind) && kind is not null && kind != "Tests")
            {
                metadata.TryGetValue("Module", out var module);
                assemblies.Add(new ProductionAssembly(assembly, assembly.GetName().Name!, kind, string.IsNullOrEmpty(module) ? null : module));
            }
        }

        var architecture = new ArchUnitNET.Loader.ArchLoader().LoadAssemblies([.. assemblies.Select(a => a.Assembly)]).Build();
        return new Loaded(assemblies, architecture);
    }

    private sealed record Loaded(IReadOnlyList<ProductionAssembly> Assemblies, ArchModel Model);
}

internal sealed record ProductionAssembly(Assembly Assembly, string Name, string Kind, string? Module);

internal static class Kinds
{
    public const string SharedKernel = "SharedKernel";
    public const string Domain = "Domain";
    public const string Contracts = "Contracts";
    public const string Application = "Application";
    public const string Infrastructure = "Infrastructure";
    public const string PlatformApplication = "Platform.Application";
    public const string PlatformInfrastructure = "Platform.Infrastructure";
    public const string Api = "Api";

    public static readonly string[] All = [SharedKernel, Domain, Contracts, Application, Infrastructure, PlatformApplication, PlatformInfrastructure, Api];
}
