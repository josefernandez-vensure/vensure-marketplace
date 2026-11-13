using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Architecture.Tests;

/// <summary>
/// No committed configuration file holds a secret: a credential slot holds a reference, such as <c>%NUGET_PAT%</c>, never a value.
/// A key generated for a test is exempt only by its line, <c>&lt;file&gt; &lt;key&gt;</c>, in <c>tests/test-keys.txt</c>.
/// </summary>
public sealed partial class SecretTests
{
    private static readonly string[] Skipped = ["bin", "obj", ".git", ".vs", "TestResults", "node_modules"];

    [Fact]
    public void No_committed_configuration_file_holds_a_secret()
    {
        var root = typeof(SecretTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "SolutionDirectory").Value!;
        var exempt = TestKeys(root);
        var violations = new List<string>();

        foreach (var file in Files(root))
        {
            var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            var name = Path.GetFileName(file);
            var found = name.Equals("nuget.config", StringComparison.OrdinalIgnoreCase) ? NuGet(file)
                : name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? Json(JsonNode.Parse(File.ReadAllText(file), documentOptions: new() { CommentHandling = System.Text.Json.JsonCommentHandling.Skip, AllowTrailingCommas = true }), string.Empty)
                : Lines(File.ReadAllLines(file));
            violations.AddRange(found.Where(key => !exempt.Contains($"{relative} {key}")).Select(key => $"{relative}: {key} holds a literal secret"));
        }

        violations.ShouldHaveNoViolations("No secret in a committed file. Reference it: user secrets locally, Key Vault above that, an environment-variable macro in nuget.config.");
    }

    private static IEnumerable<string> Files(string root) =>
        Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(f => !Path.GetRelativePath(root, f).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(Skipped.Contains))
            .Where(f => Swept().IsMatch(Path.GetFileName(f)));

    private static HashSet<string> TestKeys(string root)
    {
        var path = Path.Combine(root, "tests", "test-keys.txt");
        return File.Exists(path)
            ? File.ReadAllLines(path).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith('#')).ToHashSet()
            : [];
    }

    /// <summary>Every key path whose name says it holds a secret and whose value is a literal, or a connection string carrying one.</summary>
    private static IEnumerable<string> Json(JsonNode? node, string path)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var (key, value) in obj)
                {
                    var child = path.Length == 0 ? key : $"{path}:{key}";
                    if (value is JsonValue leaf && leaf.TryGetValue<string>(out var text) && IsLiteralSecret(key, text, path))
                    {
                        yield return child;
                    }

                    foreach (var found in Json(value, child))
                    {
                        yield return found;
                    }
                }

                break;
            case JsonArray array:
                for (var i = 0; i < array.Count; i++)
                {
                    foreach (var found in Json(array[i], $"{path}:{i}"))
                    {
                        yield return found;
                    }
                }

                break;
        }
    }

    /// <summary>Environment files and compose files: <c>KEY=value</c> and <c>KEY: value</c>.</summary>
    private static IEnumerable<string> Lines(IEnumerable<string> lines) =>
        lines.Select(l => Assignment().Match(l)).Where(m => m.Success)
            .Where(m => IsLiteralSecret(m.Groups["key"].Value, m.Groups["value"].Value.Trim().Trim('"', '\''), string.Empty))
            .Select(m => m.Groups["key"].Value);

    /// <summary>A package-source credential or API key that isn't an environment-variable macro.</summary>
    private static IEnumerable<string> NuGet(string file)
    {
        var config = XDocument.Load(file).Root!;
        var credentials = config.Element("packageSourceCredentials")?.Elements().SelectMany(source => source.Elements("add")
            .Where(add => (string?)add.Attribute("key") is "Password" or "ClearTextPassword")
            .Select(add => (Key: $"packageSourceCredentials:{source.Name.LocalName}:{add.Attribute("key")!.Value}", Value: (string?)add.Attribute("value")))) ?? [];
        var apiKeys = config.Element("apikeys")?.Elements("add").Select(add => (Key: $"apikeys:{add.Attribute("key")?.Value}", Value: (string?)add.Attribute("value"))) ?? [];
        return credentials.Concat(apiKeys).Where(c => !string.IsNullOrEmpty(c.Value) && !Macro().IsMatch(c.Value)).Select(c => c.Key);
    }

    private static bool IsLiteralSecret(string key, string value, string parent)
    {
        if (string.IsNullOrWhiteSpace(value) || Reference().IsMatch(value))
        {
            return false;
        }

        var connectionString = parent.EndsWith("ConnectionStrings", StringComparison.OrdinalIgnoreCase) || key.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase);
        return connectionString ? CredentialInConnectionString().IsMatch(value) : SecretKey().IsMatch(key);
    }

    [GeneratedRegex(@"^(appsettings.*\.json|launchSettings\.json|nuget\.config|\.env.*|.*\.env|(docker-)?compose.*\.ya?ml)$", RegexOptions.IgnoreCase)]
    private static partial Regex Swept();

    [GeneratedRegex(@"(password|passwd|pwd|secret|api[-_]?key|access[-_]?key|token|private[-_]?key|credential)", RegexOptions.IgnoreCase)]
    private static partial Regex SecretKey();

    [GeneratedRegex(@"(password|pwd|accountkey|sharedaccesskey|clientsecret)\s*=\s*[^;\s]", RegexOptions.IgnoreCase)]
    private static partial Regex CredentialInConnectionString();

    /// <summary>A value that points at a secret instead of holding one.</summary>
    [GeneratedRegex(@"^(\$\{[^}]+\}|%[A-Za-z0-9_]+%|@Microsoft\.KeyVault\(.*\))$")]
    private static partial Regex Reference();

    [GeneratedRegex(@"^%[A-Za-z0-9_]+%$")]
    private static partial Regex Macro();

    [GeneratedRegex(@"^\s*(export\s+)?-?\s*(?<key>[A-Za-z_][A-Za-z0-9_.:-]*)\s*[=:]\s*(?<value>.+)$")]
    private static partial Regex Assignment();
}
