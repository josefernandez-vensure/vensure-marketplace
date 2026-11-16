using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Api.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Shouldly;

namespace Api.Tests;

/// <summary>What the document carries beyond shapes and classes: the problem types, idempotency keys, and the constraints a client must hold a parameter to.</summary>
public sealed class ContractExtensionTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string NilGuid = "00000000-0000-0000-0000-000000000000";

    [Fact]
    public async Task Every_problem_type_the_server_declares_is_listed_with_its_status_title_and_audience()
    {
        var listed = ((await Document())["x-problem-types"]?.AsArray() ?? [])
            .Select(p => new ProblemType(p!["type"]!.GetValue<string>(), p["status"]!.GetValue<int>(), p["title"]!.GetValue<string>(), p["userFacing"]!.GetValue<bool>()))
            .ToList();

        var declared = ProductionAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.FieldType == typeof(ProblemType))
            .Select(f => (ProblemType)f.GetValue(null)!)
            .Distinct()
            .ToList();

        declared.ShouldContain(PlatformProblems.Validation);
        declared.Where(p => !listed.Contains(p)).Select(p => p.Uri).ShouldBeEmpty("every problem type a production assembly declares is in the document's x-problem-types, as declared");
        listed.Where(p => !declared.Contains(p)).Select(p => p.Uri).ShouldBeEmpty("x-problem-types lists only problem types the server declares");
    }

    [Fact]
    public async Task Every_keyed_operation_declares_its_idempotency_key_and_no_other_does()
    {
        var operations = Operations(await Document());
        var violations = new List<string>();

        foreach (var endpoint in Discovered())
        {
            var key = (operations[endpoint.Operation.Id]["parameters"]?.AsArray() ?? [])
                .SingleOrDefault(p => p!["in"]!.GetValue<string>() == "header" && p["name"]!.GetValue<string>() == "Idempotency-Key");
            var keyed = endpoint.Operation.Idempotency == IdempotencyMechanism.Key;
            if (keyed && key?["required"]?.GetValue<bool>() != true)
            {
                violations.Add($"{endpoint.Operation.Id} is keyed but doesn't declare a required Idempotency-Key header");
            }
            else if (!keyed && key is not null)
            {
                violations.Add($"{endpoint.Operation.Id} isn't keyed but declares an Idempotency-Key header");
            }
        }

        Discovered().ShouldContain(e => e.Operation.Idempotency == IdempotencyMechanism.Key);
        violations.ShouldBeEmpty("A keyed operation declares its Idempotency-Key header, so a client's generated call requires one.");
    }

    [Fact]
    public async Task Every_parameter_carries_the_constraints_the_server_holds_it_to()
    {
        var document = await Document();
        var operations = Operations(document);
        var violations = new List<string>();

        // An id: every GUID, wherever it appears, refuses the nil GUID and accepts any other.
        foreach (var (where, schema) in Schemas(document, "#"))
        {
            if (schema["format"]?.GetValue<string>() != "uuid")
            {
                continue;
            }

            if (schema["pattern"]?.GetValue<string>() is not { } pattern)
            {
                violations.Add($"{where} is a GUID with no pattern");
            }
            else if (Regex.IsMatch(NilGuid, pattern) || !Regex.IsMatch(Guid.NewGuid().ToString(), pattern))
            {
                violations.Add($"{where}: its pattern must refuse the nil GUID and accept any other");
            }
        }

        // A page size: 1 to the query's maximum, as the paging behavior refuses anything else.
        foreach (var endpoint in Discovered().Where(e => typeof(IPagedQuery).IsAssignableFrom(e.MessageType)))
        {
            var maximum = (int)endpoint.MessageType.GetProperty(nameof(IPagedQuery.MaxPageSize), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
            var pageSize = (operations[endpoint.Operation.Id]["parameters"]?.AsArray() ?? []).SingleOrDefault(p => p!["name"]!.GetValue<string>() == "pageSize")?["schema"];
            if (pageSize?["minimum"]?.GetValue<int>() != 1 || pageSize["maximum"]?.GetValue<int>() != maximum)
            {
                violations.Add($"{endpoint.Operation.Id}: pageSize must declare minimum 1 and maximum {maximum}");
            }
        }

        violations.ShouldBeEmpty("A parameter's schema carries what the server refuses, so the client's generated schema refuses it first.");
    }

    private async Task<JsonNode> Document() =>
        JsonNode.Parse(await factory.CreateClient().GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken))!;

    private List<DiscoveredEndpoint> Discovered()
    {
        _ = factory.Server;
        return [.. factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.Select(e => e.Metadata.GetMetadata<DiscoveredEndpoint>()).OfType<DiscoveredEndpoint>()];
    }

    private static Dictionary<string, JsonNode> Operations(JsonNode document) =>
        document["paths"]!.AsObject()
            .SelectMany(p => p.Value!.AsObject().Select(o => o.Value!))
            .ToDictionary(o => o["operationId"]!.GetValue<string>());

    /// <summary>Every schema object in the document, with where it is.</summary>
    private static IEnumerable<(string Where, JsonObject Schema)> Schemas(JsonNode node, string where)
    {
        if (node is JsonObject obj)
        {
            if (obj["type"] is not null || obj["format"] is not null)
            {
                yield return (where, obj);
            }

            foreach (var (key, child) in obj)
            {
                foreach (var found in child is null ? [] : Schemas(child, $"{where}/{key}"))
                {
                    yield return found;
                }
            }
        }
        else if (node is JsonArray array)
        {
            for (var i = 0; i < array.Count; i++)
            {
                foreach (var found in array[i] is { } item ? Schemas(item, $"{where}/{i}") : [])
                {
                    yield return found;
                }
            }
        }
    }

    /// <summary>The assemblies in the test's output that declare a ProjectKind other than Tests, found without asking the host.</summary>
    private static IEnumerable<Assembly> ProductionAssemblies() =>
        Directory.GetFiles(AppContext.BaseDirectory, "*.dll")
            .Select(path =>
            {
                try
                {
                    return Assembly.LoadFrom(path);
                }
                catch (BadImageFormatException)
                {
                    return null;
                }
            })
            .OfType<Assembly>()
            .Where(a => a.GetCustomAttributes<AssemblyMetadataAttribute>().Any(m => m.Key == "ProjectKind" && !string.IsNullOrEmpty(m.Value) && m.Value != "Tests"));
}
