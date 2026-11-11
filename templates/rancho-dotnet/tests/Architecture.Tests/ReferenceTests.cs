using System.Reflection;
using System.Runtime.CompilerServices;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

/// <summary>
/// What each project may depend on, at the type level. The project and package half is enforced
/// at build time by build/ProjectKinds.targets; these catch what arrives transitively or through the BCL.
/// </summary>
public sealed class ReferenceTests
{
    [Fact]
    public void Only_Api_uses_AspNetCore_beyond_authorization_and_data_protection()
    {
        var allowed = new Dictionary<string, string[]>
        {
            [Kinds.PlatformApplication] = ["Microsoft.AspNetCore.Authorization"],
            [Kinds.Infrastructure] = ["Microsoft.AspNetCore.Authorization", "Microsoft.AspNetCore.DataProtection"],
            [Kinds.PlatformInfrastructure] = ["Microsoft.AspNetCore.Authorization", "Microsoft.AspNetCore.DataProtection"],
        };

        DependenciesOf(ArchTypesOf(Kinds.All.Except([Kinds.Api]).ToArray()))
            .Where(d => InNamespace(d.Target, "Microsoft.AspNetCore"))
            .Where(d => !allowed.GetValueOrDefault(Owner(d.Origin)!.Kind, []).Any(ns => InNamespace(d.Target, ns)))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("Only Api uses ASP.NET Core; HttpContext and other transport types stay there.");
    }

    [Fact]
    public void Module_application_uses_no_data_access()
    {
        DependenciesOf(ArchTypesOf(Kinds.Application))
            .Where(d => InNamespace(d.Target, "System.Data") || InNamespace(d.Target, "Microsoft.Data") || InNamespace(d.Target, "Microsoft.EntityFrameworkCore"))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("A module's Application reads and writes only through ports; data access lives in Infrastructure.");
    }

    [Fact]
    public void A_module_reaches_another_module_only_through_its_contracts()
    {
        DependenciesOf(Model.Types)
            .Select(d => (d.Origin, d.Target, From: Owner(d.Origin), To: Owner(d.Target)))
            .Where(d => d.From?.Module is not null && d.To?.Module is not null && d.From.Module != d.To.Module && d.To.Kind != Kinds.Contracts)
            .Select(d => $"{d.Origin.FullName} ({d.From!.Module}) uses {d.Target.FullName} ({d.To!.Module} {d.To.Kind})")
            .ShouldHaveNoViolations("A module reaches another module only through that module's Contracts.");
    }

    [Fact]
    public void Api_names_only_registration_entry_points_of_infrastructure()
    {
        DependenciesOf(ArchTypesOf(Kinds.Api))
            .Where(d => Owner(d.Target)?.Kind is Kinds.Infrastructure or Kinds.PlatformInfrastructure)
            .Where(d => !IsRegistrationEntryPoint(Owner(d.Target)!.Assembly.GetType(d.Target.FullName)!))
            .Where(d => !IsSourceGenerated(d.Origin))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("Api calls each Infrastructure only through its registration entry point; dispatch reaches handlers.");
    }

    [Fact]
    public void Each_module_infrastructure_has_one_registration_entry_point()
    {
        OfKind(Kinds.Infrastructure)
            .SelectMany(a =>
            {
                var entryPoints = a.Assembly.GetExportedTypes()
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    .Where(m => m.GetParameters().Any(p => p.ParameterType.FullName == "Microsoft.Extensions.DependencyInjection.IServiceCollection"))
                    .ToList();

                var expected = $"Add{a.Module}Module";
                if (entryPoints.Count != 1)
                {
                    return [$"{a.Name} has {entryPoints.Count} public methods taking IServiceCollection; it needs exactly one, {expected}."];
                }

                var method = entryPoints[0];
                var parameters = method.GetParameters().Select(p => p.ParameterType.FullName).ToArray();
                var ok = method.Name == expected
                    && method.IsDefined(typeof(ExtensionAttribute), false)
                    && parameters.SequenceEqual(["Microsoft.Extensions.DependencyInjection.IServiceCollection", "Microsoft.Extensions.Configuration.IConfiguration"]);
                return ok ? [] : new[] { $"{a.Name}: {method.DeclaringType!.Name}.{method.Name} must be the extension method {expected}(IServiceCollection, IConfiguration)." };
            })
            .ShouldHaveNoViolations("Each module's Infrastructure exposes one entry point, Add<Module>Module(IServiceCollection, IConfiguration).");
    }

    [Fact]
    public void Internals_are_visible_only_to_tests()
    {
        Assemblies
            .SelectMany(a => a.Assembly.GetCustomAttributes<InternalsVisibleToAttribute>().Select(v => (a.Name, Friend: v.AssemblyName.Split(',')[0])))
            .Where(v => !v.Friend.EndsWith(".Tests", StringComparison.Ordinal) && v.Friend != "DynamicProxyGenAssembly2")
            .Select(v => $"{v.Name} exposes its internals to {v.Friend}")
            .ShouldHaveNoViolations("InternalsVisibleTo names test projects, and the mocking library's proxy assembly, only.");
    }

    private static bool IsSourceGenerated(ArchUnitNET.Domain.IType type) =>
        Owner(type)!.Assembly.GetType(type.FullName.Replace('/', '+')) is { } t
        && t.IsDefined(typeof(System.CodeDom.Compiler.GeneratedCodeAttribute), false);
}
