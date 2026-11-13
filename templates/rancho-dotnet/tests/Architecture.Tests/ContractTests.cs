using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

/// <summary>
/// What a module publishes to other modules keeps its shape. Each module's Contracts types are snapshotted as JSON schemas in
/// <c>contracts/&lt;Module&gt;.json</c>; a new type or a new optional property updates the snapshot, anything else is a new version.
/// </summary>
public sealed class ContractTests
{
    /// <summary>In CI, a directory holding the base branch's snapshots (build/check-contracts.sh): only a breaking change fails, and nothing is written.</summary>
    private const string BaselineVariable = "CONTRACT_BASELINE";

    [Fact]
    public void Published_contracts_keep_their_shape()
    {
        var baseline = Environment.GetEnvironmentVariable(BaselineVariable);
        var violations = new List<string>();
        var updated = new List<string>();

        foreach (var module in OfKind(Kinds.Contracts).GroupBy(a => a.Module!))
        {
            var current = Snapshot(module.SelectMany(a => TypesOf(a.Kind).Where(t => Owner(t) == a)));
            var path = Path.Combine(baseline ?? Path.Combine(SolutionDirectory, "contracts"), $"{module.Key}.json");
            var committed = File.Exists(path) ? JsonNode.Parse(File.ReadAllText(path))!.AsObject() : [];

            foreach (var (type, schema) in committed)
            {
                if (current[type] is not { } now)
                {
                    violations.Add($"{type} was published and is gone; a published type is never removed");
                }
                else
                {
                    violations.AddRange(Breaks(schema!, now, type));
                }
            }

            if (baseline is null && !JsonNode.DeepEquals(committed, current))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, current.ToJsonString(new JsonSerializerOptions { WriteIndented = true }).ReplaceLineEndings("\n") + "\n");
                updated.Add(path);
            }
        }

        violations.ShouldHaveNoViolations("A published contract never loses, renames or retypes a property, or gains a required one: publish a new version (V2) instead.");
        updated.ShouldHaveNoViolations("A published contract gained a type or an optional property. Its snapshot has been regenerated: review the diff and commit it.");
    }

    private static string SolutionDirectory =>
        typeof(ContractTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "SolutionDirectory").Value!;

    /// <summary>Each published type's schema as the outbox serializes it, keyed by full name.</summary>
    private static JsonObject Snapshot(IEnumerable<Type> types)
    {
        var snapshot = new JsonObject();
        foreach (var type in types.Where(t => t is { IsInterface: false, IsGenericTypeDefinition: false } && !(t.IsAbstract && t.IsSealed) && !typeof(Attribute).IsAssignableFrom(t)).OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            snapshot[type.FullName!] = JsonSerializerOptions.Default.GetJsonSchemaAsNode(type);
        }

        return snapshot;
    }

    /// <summary>What a consumer of the old shape can't read: a property gone or retyped, or one newly required.</summary>
    private static IEnumerable<string> Breaks(JsonNode before, JsonNode after, string path)
    {
        if (before is not JsonObject old || after is not JsonObject now)
        {
            if (!JsonNode.DeepEquals(before, after))
            {
                yield return $"{path} changed from {before.ToJsonString()} to {after.ToJsonString()}";
            }

            yield break;
        }

        foreach (var (key, value) in old)
        {
            if (key == "properties")
            {
                var properties = now["properties"]?.AsObject();
                foreach (var (name, schema) in value!.AsObject())
                {
                    if (properties?[name] is not { } current)
                    {
                        yield return $"{path}.{name} was removed or renamed";
                        continue;
                    }

                    foreach (var change in Breaks(schema!, current, $"{path}.{name}"))
                    {
                        yield return change;
                    }
                }
            }
            else if (key == "items" && now["items"] is { } items)
            {
                foreach (var change in Breaks(value!, items, $"{path}[]"))
                {
                    yield return change;
                }
            }
            else if (key != "required" && !JsonNode.DeepEquals(value, now[key]))
            {
                yield return $"{path}: {key} changed from {value?.ToJsonString() ?? "nothing"} to {now[key]?.ToJsonString() ?? "nothing"}";
            }
        }

        var wasRequired = (old["required"]?.AsArray() ?? []).Select(n => n!.GetValue<string>()).ToHashSet();
        foreach (var name in (now["required"]?.AsArray() ?? []).Select(n => n!.GetValue<string>()).Where(n => !wasRequired.Contains(n)))
        {
            yield return $"{path}.{name} is newly required";
        }
    }
}
