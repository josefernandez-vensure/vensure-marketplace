using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Api.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Shouldly;

namespace Api.Tests;

/// <summary>The OpenAPI document is the wire contract: committed, and checked against the one the built host generates.</summary>
public sealed class ContractDocumentTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string DataClass = "x-data-class";
    private static readonly string[] Classes = ["public", "internal", "confidential", "restricted"];

    [Fact]
    public async Task The_committed_openapi_document_is_the_one_the_host_generates()
    {
        var path = typeof(ContractDocumentTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "OpenApiDocument").Value!;
        var live = await Document();
        var committed = File.Exists(path) ? JsonNode.Parse(await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken)) : null;

        if (!JsonNode.DeepEquals(committed, live))
        {
            // Regenerate it, so the change shows in the diff. The test fails until it is committed.
            var text = live.ToJsonString(new JsonSerializerOptions { WriteIndented = true }).ReplaceLineEndings("\n") + "\n";
            await File.WriteAllTextAsync(path, text, TestContext.Current.CancellationToken);
            Assert.Fail($"{path} was not the document the host generates. It has been regenerated: review the diff and commit it.");
        }
    }

    [Fact]
    public async Task Every_property_and_parameter_schema_carries_its_data_class()
    {
        var document = await Document();
        var components = Components(document);
        var violations = new List<string>();

        foreach (var (name, schema) in components)
        {
            CheckProperties(schema!, $"#/components/schemas/{name}", components, violations);
        }

        foreach (var (route, method, operation) in Operations(document))
        {
            foreach (var parameter in operation["parameters"]?.AsArray() ?? [])
            {
                if (ClassOf(parameter!["schema"]!, components) is not { } dataClass || !Classes.Contains(dataClass))
                {
                    violations.Add($"{method} {route}: parameter {parameter["name"]} has no {DataClass}");
                }
            }

            foreach (var schema in Schemas(operation["requestBody"]).Concat(operation["responses"]!.AsObject().SelectMany(r => Schemas(r.Value))))
            {
                CheckProperties(schema, $"{method} {route}", components, violations);
            }
        }

        violations.ShouldBeEmpty($"Every property and parameter schema carries {DataClass}: public, internal, confidential or restricted. A $ref takes its component's.");
    }

    [Fact]
    public async Task Every_array_a_success_returns_is_bounded_and_a_page_by_its_maximum_page_size()
    {
        var document = await Document();
        var components = Components(document);
        var maxima = PagedOperations();
        var violations = new List<string>();

        foreach (var (_, _, operation) in Operations(document))
        {
            var operationId = operation["operationId"]!.GetValue<string>();
            var successes = operation["responses"]!.AsObject().Where(r => r.Key.StartsWith('2')).SelectMany(r => Schemas(r.Value)).ToList();

            violations.AddRange(successes
                .SelectMany(s => Arrays(s, "response", components, []))
                .Where(a => a.Array["maxItems"] is null)
                .Select(a => $"{operationId}: {a.Path} is an array without maxItems"));

            if (maxima.TryGetValue(operationId, out var maximum))
            {
                var bound = successes.Select(s => Resolve(s, components)["properties"]?["items"]?["maxItems"]?.GetValue<int>()).SingleOrDefault();
                if (bound is null || bound < maximum)
                {
                    violations.Add($"{operationId}: the page's items declare maxItems {bound?.ToString() ?? "none"}, below its maximum page size {maximum}");
                }
            }
        }

        maxima.ShouldNotBeEmpty();
        violations.ShouldBeEmpty("Every collection a success returns is bounded: a page's items carry maxItems, at least its query's maximum page size.");
    }

    private async Task<JsonNode> Document() =>
        JsonNode.Parse(await factory.CreateClient().GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken))!;

    private Dictionary<string, int> PagedOperations()
    {
        _ = factory.Server;
        return factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .Select(e => e.Metadata.GetMetadata<DiscoveredEndpoint>())
            .OfType<DiscoveredEndpoint>()
            .Where(d => typeof(IPagedQuery).IsAssignableFrom(d.MessageType))
            .ToDictionary(d => d.Operation.Id, d => (int)d.MessageType.GetProperty(nameof(IPagedQuery.MaxPageSize), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!);
    }

    private static JsonObject Components(JsonNode document) => document["components"]?["schemas"]?.AsObject() ?? [];

    private static IEnumerable<(string Route, string Method, JsonNode Operation)> Operations(JsonNode document) =>
        document["paths"]!.AsObject().SelectMany(p => p.Value!.AsObject().Select(o => (p.Key, o.Key.ToUpperInvariant(), o.Value!)));

    private static IEnumerable<JsonNode> Schemas(JsonNode? owner) =>
        (owner?["content"]?.AsObject() ?? []).Select(c => c.Value?["schema"]).OfType<JsonNode>();

    private static void CheckProperties(JsonNode schema, string where, JsonObject components, List<string> violations)
    {
        foreach (var (name, property) in schema["properties"]?.AsObject() ?? [])
        {
            if (ClassOf(property!, components) is not { } dataClass || !Classes.Contains(dataClass))
            {
                violations.Add($"{where}: property {name} has no {DataClass}");
            }

            if (property!["$ref"] is null)
            {
                CheckProperties(property, $"{where}.{name}", components, violations);
                if (property["items"] is { } items && items["$ref"] is null)
                {
                    CheckProperties(items, $"{where}.{name}[]", components, violations);
                }
            }
        }
    }

    /// <summary>A schema's class: its own, or, for a $ref, its component's.</summary>
    private static string? ClassOf(JsonNode schema, JsonObject components) =>
        schema[DataClass]?.GetValue<string>() ?? (schema["$ref"] is not null ? Resolve(schema, components)[DataClass]?.GetValue<string>() : null);

    private static JsonNode Resolve(JsonNode schema, JsonObject components) =>
        schema["$ref"]?.GetValue<string>() is { } reference ? components[reference.Split('/')[^1]]! : schema;

    private static IEnumerable<(string Path, JsonNode Array)> Arrays(JsonNode schema, string path, JsonObject components, HashSet<string> seen)
    {
        if (schema["$ref"]?.GetValue<string>() is { } reference)
        {
            if (!seen.Add(reference))
            {
                yield break;
            }

            schema = Resolve(schema, components);
        }

        if (schema["type"]?.ToJsonString().Contains("\"array\"", StringComparison.Ordinal) == true)
        {
            yield return (path, schema);
        }

        var children = (schema["properties"]?.AsObject() ?? []).Select(p => (Path: $"{path}.{p.Key}", Node: p.Value))
            .Append(($"{path}[]", schema["items"]))
            .Append(($"{path}{{}}", schema["additionalProperties"] as JsonObject));
        foreach (var (childPath, child) in children)
        {
            if (child is null)
            {
                continue;
            }

            foreach (var found in Arrays(child, childPath, components, seen))
            {
                yield return found;
            }
        }
    }
}
