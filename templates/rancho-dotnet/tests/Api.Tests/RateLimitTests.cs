using System.Net;
using System.Text.Json;
using Api.Endpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Shouldly;

namespace Api.Tests;

/// <summary>Every rate-limit policy the routes use, read from the routes the host actually maps.</summary>
public sealed class RateLimitTests
{
    [Fact]
    public async Task Every_rate_limit_refuses_a_request_beyond_it_with_problem_details_and_retry_after()
    {
        using var factory = new ApiFactory().WithWebHostBuilder(b => b
            .UseSetting("RateLimits:PermitLimit", "1")
            .UseSetting("RateLimits:Window", "00:10:00"));
        _ = factory.Server;
        var routes = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
            .Select(r => (Route: r, Discovered: r.Metadata.GetMetadata<DiscoveredEndpoint>()))
            .Where(r => r.Discovered is not null)
            .DistinctBy(r => r.Discovered!.Operation.RateLimit)
            .ToList();
        var violations = new List<string>();

        // An anonymous caller: the limiter runs before authorization, so a refused request spends a permit too.
        var client = factory.CreateClient();
        foreach (var (route, discovered) in routes)
        {
            await client.SendAsync(Request(route), TestContext.Current.CancellationToken);
            var refused = await client.SendAsync(Request(route), TestContext.Current.CancellationToken);

            if (refused.StatusCode != HttpStatusCode.TooManyRequests
                || await ProblemType(refused) != PlatformProblems.RateLimited.Uri
                || refused.Headers.RetryAfter is null)
            {
                violations.Add($"rate limit '{discovered!.Operation.RateLimit}' on {discovered.Operation.Id} answered {(int)refused.StatusCode}, Retry-After {refused.Headers.RetryAfter?.ToString() ?? "missing"}");
            }
        }

        routes.ShouldNotBeEmpty();
        violations.ShouldBeEmpty("A request beyond its limit is 429 problem details, with Retry-After where the limiter can estimate it.");
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

    private static async Task<string?> ProblemType(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentType?.MediaType != "application/problem+json")
        {
            return null;
        }

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return body.RootElement.TryGetProperty("type", out var type) ? type.GetString() : null;
    }
}
