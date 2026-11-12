using System.Net;
using Api.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

/// <summary>Every operation's policy, over every route the host maps: a principal it doesn't admit is refused and the refusal recorded.</summary>
public sealed class PolicyTests(SqlServer sql)
{
    [Fact]
    public async Task Every_operation_refuses_a_principal_its_policy_does_not_admit()
    {
        var subject = $"nobody-{Guid.NewGuid():N}";
        var client = sql.Api.ClientFor(subject);
        var violations = new List<string>();

        _ = sql.Api.Server;
        var routes = sql.Api.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
            .Select(r => (Route: r, Discovered: r.Metadata.GetMetadata<DiscoveredEndpoint>()))
            .Where(r => r.Discovered is not null)
            .ToList();
        foreach (var (route, discovered) in routes)
        {
            var response = await client.SendAsync(Request(route), Cancel);
            if (response.StatusCode != HttpStatusCode.Forbidden || await ProblemType(response) != PlatformProblems.Forbidden.Uri)
            {
                violations.Add($"{discovered!.Operation.Id} ({discovered.Operation.Policy}) answered {(int)response.StatusCode} to a principal without its permission");
            }
        }

        routes.ShouldNotBeEmpty();
        violations.ShouldBeEmpty("Every operation's policy refuses a principal it does not admit, as 403 problem details.");
        (await sql.Scalar<int>($"SELECT COUNT(*) AS Value FROM catalog.AuditTrail WHERE PrincipalId = {subject} AND Outcome = 'Denied'"))
            .ShouldBe(routes.Count);
    }

    [Fact]
    public async Task A_principal_with_the_permission_but_no_tenant_is_refused()
    {
        var response = await sql.Api.ClientFor("tenantless", Guid.Empty, "catalog.read").GetAsync("/catalog/products", Cancel);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    /// <summary>A request to the route with its parameters filled; a refusal doesn't read the body.</summary>
    private static HttpRequestMessage Request(RouteEndpoint route)
    {
        var path = string.Concat(route.RoutePattern.PathSegments.Select(segment => "/" + string.Concat(segment.Parts.Select(part => part switch
        {
            RoutePatternLiteralPart literal => literal.Content,
            RoutePatternParameterPart => Guid.NewGuid().ToString(),
            _ => string.Empty,
        }))));
        var method = route.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods[0];
        return new HttpRequestMessage(new HttpMethod(method), path);
    }
}
