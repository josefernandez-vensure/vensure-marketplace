using Api.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Api.Tests;

/// <summary>What the built host actually maps, read from its endpoint data source.</summary>
public sealed class EndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    /// <summary>Framework endpoints, mapped outside discovery. Each carries its own policy or falls to the denying fallback.</summary>
    private static readonly string[] FrameworkRoutes = ["/openapi/{documentName}.json"];

    [Fact]
    public void Every_route_is_a_discovered_endpoint_or_a_declared_framework_endpoint()
    {
        Routes()
            .Where(r => r.Metadata.GetMetadata<DiscoveredEndpoint>() is null && !FrameworkRoutes.Contains(r.RoutePattern.RawText))
            .Select(r => r.RoutePattern.RawText)
            .ShouldBeEmpty("Endpoints are registered by discovery; hand-registering one is a defect.");
    }

    [Fact]
    public void Every_endpoint_type_is_mapped()
    {
        var mapped = Routes().Select(r => r.Metadata.GetMetadata<DiscoveredEndpoint>()?.EndpointType).ToHashSet();

        EndpointRegistrar.Discover().Select(d => d.Endpoint).Where(t => !mapped.Contains(t)).Select(t => t.Name)
            .ShouldBeEmpty();
    }

    [Fact]
    public void Every_endpoint_carries_its_operations_policy_and_rate_limit()
    {
        foreach (var route in Routes().Where(r => r.Metadata.GetMetadata<DiscoveredEndpoint>() is not null))
        {
            var operation = route.Metadata.GetRequiredMetadata<DiscoveredEndpoint>().Operation;
            var policies = route.Metadata.GetOrderedMetadata<IAuthorizeData>();

            policies.ShouldNotBeEmpty($"{route.RoutePattern.RawText} has no authorization");
            policies.ShouldAllBe(a => a.Policy != null || a.Roles != null || a.AuthenticationSchemes != null, $"{route.RoutePattern.RawText} has a bare [Authorize]");
            policies.Select(a => a.Policy).ShouldContain(operation.Policy);
            route.Metadata.GetMetadata<DisableRateLimitingAttribute>().ShouldBeNull();
            route.Metadata.GetMetadata<EnableRateLimitingAttribute>().ShouldNotBeNull($"{route.RoutePattern.RawText} has no rate limit").PolicyName.ShouldBe(operation.RateLimit);
        }
    }

    [Fact]
    public async Task The_fallback_policy_denies_everyone()
    {
        using var scope = factory.Services.CreateScope();
        var fallback = scope.ServiceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value.FallbackPolicy;
        fallback.ShouldNotBeNull();

        var authorization = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        (await authorization.AuthorizeAsync(new System.Security.Claims.ClaimsPrincipal(), fallback)).Succeeded.ShouldBeFalse();
        (await authorization.AuthorizeAsync(ApiFactory.Principal("someone", "catalog.read", "catalog.write"), fallback)).Succeeded.ShouldBeFalse();
    }

    [Fact]
    public void Every_operation_policy_is_registered()
    {
        var options = factory.Services.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        Routes().Select(r => r.Metadata.GetMetadata<DiscoveredEndpoint>()?.Operation).OfType<Platform.Application.Abstractions.Operation>()
            .Where(o => options.GetPolicy(o.Policy) is null)
            .Select(o => $"{o.Id} names policy {o.Policy}, which nothing registers")
            .ShouldBeEmpty();
    }

    private List<RouteEndpoint> Routes()
    {
        _ = factory.Server;
        return factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>().ToList();
    }
}
