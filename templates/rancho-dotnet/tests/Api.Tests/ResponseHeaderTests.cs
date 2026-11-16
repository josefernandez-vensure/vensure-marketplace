using System.Reflection;
using System.Text.Json.Nodes;
using Api.Endpoints;
using Api.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using SharedKernel;
using Shouldly;

namespace Api.Tests;

/// <summary>What every response tells a cache and a browser on another origin.</summary>
public sealed class ResponseHeaderTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Origin = "https://app.example.com";

    [Fact]
    public async Task Every_route_sends_cache_control_by_the_class_of_what_it_returns()
    {
        var violations = new List<string>();
        foreach (var (endpoint, method, path) in Routes())
        {
            // An anonymous caller is refused; the refusal carries the route's header like any other answer.
            var response = await factory.CreateClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), path), TestContext.Current.CancellationToken);
            var expected = CachePolicy.For(endpoint);
            var sent = response.Headers.CacheControl?.ToString();
            if (expected is null || sent != expected)
            {
                violations.Add($"{method} {path}: Cache-Control is '{sent}', expected '{expected}'");
            }
        }

        violations.ShouldBeEmpty("Every route's answer is private, and no-store where its success carries Confidential or Restricted data.");
    }

    [Fact]
    public void A_response_carrying_confidential_restricted_or_unclassified_data_is_never_stored()
    {
        CachePolicy.For(typeof(InternalView)).ShouldBe(CachePolicy.Private);
        CachePolicy.For(typeof(Page<InternalView>)).ShouldBe(CachePolicy.Private);
        CachePolicy.For(typeof(ConfidentialView)).ShouldBe(CachePolicy.NoStore);
        CachePolicy.For(typeof(Page<ConfidentialView>)).ShouldBe(CachePolicy.NoStore);
        CachePolicy.For(typeof(RestrictedView)).ShouldBe(CachePolicy.NoStore);
        CachePolicy.For(typeof(UnclassifiedView)).ShouldBe(CachePolicy.NoStore);
    }

    [Fact]
    public async Task A_listed_origin_may_send_every_request_header_the_contract_uses_and_read_every_response_header()
    {
        using var listed = factory.WithWebHostBuilder(b => b.UseSetting("Cors:Origins:0", Origin));
        var headers = ContractRequestHeaders();
        headers.ShouldContain("Idempotency-Key");

        using var preflight = new HttpRequestMessage(HttpMethod.Options, $"/catalog/products/{Guid.NewGuid()}/stock-receipts");
        preflight.Headers.Add("Origin", Origin);
        preflight.Headers.Add("Access-Control-Request-Method", "POST");
        preflight.Headers.Add("Access-Control-Request-Headers", string.Join(", ", headers));
        var answer = await listed.CreateClient().SendAsync(preflight, TestContext.Current.CancellationToken);

        answer.Headers.GetValues("Access-Control-Allow-Origin").ShouldBe([Origin]);
        var allowed = answer.Headers.GetValues("Access-Control-Allow-Headers").SelectMany(v => v.Split(',', StringSplitOptions.TrimEntries)).ToList();
        headers.Where(h => !allowed.Contains(h, StringComparer.OrdinalIgnoreCase)).ShouldBeEmpty("every request header the contract uses is admitted");

        using var read = new HttpRequestMessage(HttpMethod.Get, $"/catalog/products/{Guid.NewGuid()}");
        read.Headers.Add("Origin", Origin);
        var response = await listed.CreateClient().SendAsync(read, TestContext.Current.CancellationToken);
        var exposed = response.Headers.GetValues("Access-Control-Expose-Headers").SelectMany(v => v.Split(',', StringSplitOptions.TrimEntries)).ToList();
        exposed.ShouldContain("ETag");
        exposed.ShouldContain("Retry-After");
    }

    [Fact]
    public async Task An_origin_not_listed_is_admitted_to_nothing()
    {
        using var listed = factory.WithWebHostBuilder(b => b.UseSetting("Cors:Origins:0", Origin));
        using var preflight = new HttpRequestMessage(HttpMethod.Options, "/catalog/products");
        preflight.Headers.Add("Origin", "https://elsewhere.example.com");
        preflight.Headers.Add("Access-Control-Request-Method", "GET");

        var answer = await listed.CreateClient().SendAsync(preflight, TestContext.Current.CancellationToken);

        answer.Headers.Contains("Access-Control-Allow-Origin").ShouldBeFalse();
    }

    /// <summary>The headers a client sends: every header parameter of the committed document, credentials, bodies and trace context.</summary>
    private static List<string> ContractRequestHeaders()
    {
        var path = typeof(ResponseHeaderTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "OpenApiDocument").Value!;
        var document = JsonNode.Parse(File.ReadAllText(path))!;
        return [.. document["paths"]!.AsObject()
            .SelectMany(p => p.Value!.AsObject())
            .SelectMany(o => o.Value!["parameters"]?.AsArray() ?? [])
            .Where(p => p!["in"]!.GetValue<string>() == "header")
            .Select(p => p!["name"]!.GetValue<string>())
            .Concat(["Authorization", "Content-Type", "traceparent", "tracestate"])
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    private IEnumerable<(Endpoint Endpoint, string Method, string Path)> Routes()
    {
        _ = factory.Server;
        foreach (var endpoint in factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>())
        {
            if (endpoint.Metadata.GetMetadata<DiscoveredEndpoint>() is null)
            {
                continue;
            }

            var path = "/" + string.Join('/', endpoint.RoutePattern.PathSegments.Select(s => string.Concat(s.Parts.Select(part => part switch
            {
                RoutePatternLiteralPart literal => literal.Content,
                RoutePatternParameterPart => Guid.NewGuid().ToString(),
                _ => string.Empty,
            }))));
            foreach (var method in endpoint.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods)
            {
                yield return (endpoint, method, path);
            }
        }
    }

    [Internal]
    public sealed record InternalView(string Value);

    [Confidential]
    public sealed record ConfidentialView(string Value)
    {
        public override string ToString() => "***";
    }

    [Restricted]
    public sealed record RestrictedView(string Value)
    {
        public override string ToString() => "***";
    }

    public sealed record UnclassifiedView(string Value);
}
