using static Architecture.Tests.Solution;

namespace Architecture.Tests;

public sealed class EndpointTests
{
    private static IEnumerable<Type> Endpoints => TypesOf(Kinds.Api).Where(t => !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.Name == "IEndpoint`1"));

    [Fact]
    public void Only_the_registrar_maps_a_route_to_a_handler()
    {
        // Every overload that takes a handler delegate lives on these classes; framework endpoints use their own extensions.
        string[] mappers = ["Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions", "Microsoft.AspNetCore.Builder.RouteHandlerServices"];

        Model.Types
            .Where(t => Owner(t)?.Kind == Kinds.Api && !t.FullName.StartsWith("Api.Endpoints.EndpointRegistrar", StringComparison.Ordinal))
            .SelectMany(t => t.Dependencies.OfType<ArchUnitNET.Domain.Dependencies.MethodCallDependency>().Select(d => (t, d)))
            .Where(x => mappers.Contains(x.d.TargetMember.DeclaringType.FullName))
            .Select(x => $"{x.t.FullName} calls {x.d.TargetMember.Name}")
            .ShouldHaveNoViolations("Every endpoint is a type the registrar discovers; a lambda or method group passed to MapGet, MapPost or MapMethods is forbidden.");
    }

    [Fact]
    public void Endpoints_reference_only_their_messages_and_dtos()
    {
        var violations = new List<string>();
        foreach (var endpoint in Endpoints)
        {
            var message = endpoint.GetInterfaces().Single(i => i.IsGenericType && i.Name == "IEndpoint`1").GetGenericArguments()[0];
            var slice = message.Namespace;

            foreach (var (origin, target) in DependenciesOf(WithNested(endpoint)))
            {
                var owner = Owner(target);
                if (owner?.Kind is Kinds.Domain or Kinds.Infrastructure or Kinds.PlatformInfrastructure
                    || (owner?.Kind == Kinds.Application && !target.FullName.StartsWith(slice + ".", StringComparison.Ordinal)))
                {
                    violations.Add($"{origin.FullName} uses {target.FullName}");
                }
            }
        }

        violations.ShouldHaveNoViolations("An endpoint maps its request to its message, dispatches it, and translates the result; it references nothing else of the application.");
    }
}
