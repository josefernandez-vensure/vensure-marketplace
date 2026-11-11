using static Architecture.Tests.Solution;

namespace Architecture.Tests;

public sealed class InfrastructureTests
{
    [Fact]
    public void Infrastructure_is_organized_by_technical_concern()
    {
        string[] concerns = ["Persistence", "Queries", "Messaging", "External"];

        OfKind(Kinds.Infrastructure, Kinds.PlatformInfrastructure)
            .SelectMany(a => TypesOf(a.Kind).Where(t => Owner(t) == a).Select(t => (a.Name, Type: t)))
            .Where(x => !IsRegistrationEntryPoint(x.Type))
            .Where(x => !concerns.Any(c => x.Type.Namespace == $"{x.Name}.{c}" || x.Type.Namespace?.StartsWith($"{x.Name}.{c}.", StringComparison.Ordinal) == true))
            .Select(x => $"{x.Type.FullName} is outside {string.Join(", ", concerns.Select(c => $"{x.Name}.{c}"))}")
            .ShouldHaveNoViolations("Infrastructure is organized by technical concern; only the registration entry point sits at the root.");
    }

    [Fact]
    public void Application_is_sliced_by_feature_and_domain_by_aggregate()
    {
        // IDE0130 makes every namespace match its folder, so a namespace names the folder.
        var byPattern = new Dictionary<string, string[]>
        {
            [Kinds.Application] = ["Handlers", "Commands", "Queries", "Validators", "Dtos"],
            [Kinds.Domain] = ["Entities", "ValueObjects", "Events", "Repositories"],
        };

        byPattern
            .SelectMany(kv => OfKind(kv.Key).SelectMany(a => TypesOf(a.Kind).Where(t => Owner(t) == a)
                .Where(t => kv.Value.Any(p => t.Namespace == $"{a.Name}.{p}" || t.Namespace?.StartsWith($"{a.Name}.{p}.", StringComparison.Ordinal) == true))
                .Select(t => $"{t.FullName} is grouped by pattern")))
            .ShouldHaveNoViolations("Application is sliced by feature (Application/<Feature>/), Domain by aggregate; never by pattern (Handlers/, Entities/).");
    }

    [Fact]
    public void Only_the_composition_root_reads_configuration_or_the_environment()
    {
        string[] configuration = ["Microsoft.Extensions.Configuration.IConfiguration", "Microsoft.Extensions.Configuration.IConfigurationRoot",
            "Microsoft.Extensions.Configuration.IConfigurationSection", "Microsoft.Extensions.Configuration.IConfigurationManager",
            "Microsoft.Extensions.Configuration.ConfigurationManager", "Microsoft.Extensions.Configuration.IConfigurationBuilder",
            "Microsoft.Extensions.Configuration.ConfigurationBuilder", "Microsoft.Extensions.Hosting.IHostEnvironment",
            "Microsoft.AspNetCore.Hosting.IWebHostEnvironment", "Microsoft.Extensions.Hosting.Environments",
            "Microsoft.Extensions.Hosting.HostEnvironmentEnvExtensions"];

        DependenciesOf(ArchTypesOf(Kinds.All.Except([Kinds.Api]).ToArray()))
            .Where(d => configuration.Contains(d.Target.FullName))
            .Where(d => !IsInRegistrationEntryPoint(d.Origin))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("Configuration binds to options classes at the composition root: Api and the registration entry points.");
    }

    [Fact]
    public void Only_the_composition_root_resolves_services()
    {
        string[] locators = ["System.IServiceProvider", "Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions",
            "Microsoft.Extensions.DependencyInjection.ServiceProviderKeyedServiceExtensions", "Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider",
            "Microsoft.Extensions.DependencyInjection.ActivatorUtilities"];

        DependenciesOf(ArchTypesOf(Kinds.Infrastructure, Kinds.PlatformInfrastructure))
            .Where(d => locators.Contains(d.Target.FullName))
            .Where(d => !IsInRegistrationEntryPoint(d.Origin))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("Dependencies come through the constructor; only the composition root resolves services. A hosted service creates a scope through IServiceScopeFactory.");
    }

    private static bool IsInRegistrationEntryPoint(ArchUnitNET.Domain.IType type)
    {
        var outer = type.FullName.Split('+', '/')[0];
        return Owner(type)!.Assembly.GetType(outer) is { } t && IsRegistrationEntryPoint(t);
    }
}
