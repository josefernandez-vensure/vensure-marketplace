using System.CodeDom.Compiler;
using System.Reflection;
using System.Xml.Linq;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

/// <summary>
/// Every rule checks only what discovery hands it. These prove discovery found every project, and that each set a rule walks is
/// not empty, so a green suite means the rules ran.
/// </summary>
public sealed class DiscoveryTests
{
    [Fact]
    public void Every_production_project_is_scanned()
    {
        var root = ContractTests.SolutionDirectory;
        var solution = Directory.GetFiles(root, "*.slnx").Single();
        var listed = XDocument.Load(solution).Descendants("Project")
            .Select(p => Path.GetFullPath(Path.Combine(root, (string)p.Attribute("Path")!)))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var scanned = Assemblies.Select(a => a.Name).ToHashSet(StringComparer.Ordinal);
        var violations = new List<string>();

        foreach (var project in Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
                     .Where(p => !p.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(d => d is "bin" or "obj")))
        {
            var name = Path.GetFileNameWithoutExtension(project);
            if (!listed.Contains(Path.GetFullPath(project)))
            {
                violations.Add($"{name} is not in {Path.GetFileName(solution)}: add it with `dotnet sln add`, so the build and every check reach it");
                continue;
            }

            var xml = XDocument.Load(project).Descendants();
            var kind = xml.FirstOrDefault(e => e.Name.LocalName == "ProjectKind")?.Value;
            var assembly = xml.FirstOrDefault(e => e.Name.LocalName == "AssemblyName")?.Value ?? name;
            if (kind is not "Tests" && !scanned.Contains(assembly))
            {
                var underSrc = Path.GetRelativePath(root, project).StartsWith("src" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                violations.Add(underSrc
                    ? $"{name} (ProjectKind {kind ?? "none"}) is not scanned: its assembly carries no ProjectKind metadata. The build writes it as an assembly "
                        + "attribute, so keep <GenerateAssemblyInfo> true"
                    : $"{name} (ProjectKind {kind ?? "none"}) is not scanned: Architecture.Tests references src/**/*.csproj, so move the project under src/");
            }
        }

        violations.ShouldHaveNoViolations("Every production project in the solution is scanned by the architecture tests.");
    }

    [Fact]
    public void Only_a_generator_marks_code_generated()
    {
        // A tool is named without its version. Add a generator here when the solution takes one on.
        string[] generators =
        [
            "Mediator.SourceGenerator", "Microsoft.AspNetCore.OpenApi.SourceGenerators", "Microsoft.Extensions.Logging.Generators", "Microsoft.Gen.Logging",
            "System.Text.Json.SourceGeneration", "System.Text.RegularExpressions.Generator", "Microsoft.Extensions.Options.SourceGeneration",
            "Microsoft.Extensions.Configuration.Binder.SourceGeneration",
        ];

        Assemblies.SelectMany(a => a.Assembly.GetTypes())
            .Select(t => (Type: t, Attribute: t.GetCustomAttribute<GeneratedCodeAttribute>(false)))
            .Where(x => x.Attribute is not null && !generators.Contains(x.Attribute.Tool?.Split(',')[0]))
            .Select(x => $"{x.Type.FullName} is marked [GeneratedCode(\"{x.Attribute!.Tool}\")], which no known generator writes")
            .ShouldHaveNoViolations("Every rule skips generated code, so only a source generator marks a type generated; hand-written code never carries [GeneratedCode].");
    }

    [Fact]
    public void Every_rule_has_something_to_check()
    {
        (string What, int Count)[] sets =
        [
            .. new[] { Kinds.SharedKernel, Kinds.Domain, Kinds.Application, Kinds.Infrastructure, Kinds.PlatformApplication, Kinds.PlatformInfrastructure, Kinds.Api }
                .Select(kind => ($"a {kind} assembly", OfKind(kind).Count())),
            ("an aggregate root (IAggregateRoot in Domain)", DomainTests.AggregateRoots.Count()),
            ("a message (ICommand or IQuery)", OperationTests.Messages.Count),
            ("a handler (Mediator's handler interfaces)", ApplicationTests.Handlers.Count()),
            ("a query handler (IQueryHandler<,>)", ApplicationTests.QueryHandlers.Count()),
            ("a query response", ClassificationTests.QueryResponses.Count()),
            ("an endpoint (IEndpoint<T> in Api)", EndpointTests.Endpoints.Count()),
            ("a logging method ([LoggerMessage])", ClassificationTests.LoggingMethods.Count()),
        ];

        sets.Where(s => s.Count == 0)
            .Select(s => $"found no {s.What}: every rule over that set passes without checking anything. Discovery, or the type it looks for, has changed")
            .ShouldHaveNoViolations("Every set a rule walks is found.");
    }
}
