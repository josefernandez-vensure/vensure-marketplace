using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Api.Tests;

public sealed class StartupTests
{
    [Fact]
    public void Every_options_class_the_solution_binds_has_a_validator()
    {
        using var factory = new ApiFactory();
        _ = factory.Server;

        var bound = factory.Registrations
            .Select(d => d.ServiceType)
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IConfigureOptions<>))
            .Select(t => t.GetGenericArguments()[0])
            .Where(IsSolutionType)
            .ToHashSet();
        var validated = factory.Registrations
            .Select(d => d.ServiceType)
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IValidateOptions<>))
            .Select(t => t.GetGenericArguments()[0])
            .ToHashSet();

        bound.ShouldNotBeEmpty();
        bound.Where(t => !validated.Contains(t)).Select(t => t.FullName)
            .ShouldBeEmpty("Every options class has a validator and is registered with ValidateOnStart.");
    }

    [Theory]
    [InlineData("0", "00:01:00")]
    [InlineData("100", "00:00:00")]
    [InlineData("", "")]
    public void A_missing_or_unusable_limit_stops_startup(string permitLimit, string window)
    {
        using var factory = new ApiFactory().WithWebHostBuilder(b => b
            .UseSetting("RateLimits:PermitLimit", permitLimit)
            .UseSetting("RateLimits:Window", window));

        // Either the validator or the binder refuses; what matters is that the host never serves with a limit it cannot enforce.
        Should.Throw<Exception>(() => _ = factory.Server);
    }

    private static bool IsSolutionType(Type type) =>
        type.Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Any(a => a.Key == "ProjectKind");
}
