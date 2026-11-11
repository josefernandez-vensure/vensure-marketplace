using System.Reflection;
using System.Text.RegularExpressions;
using Platform.Application.Abstractions;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

public sealed partial class ApplicationTests
{
    private static IEnumerable<Type> QueryHandlers => TypesOf(Kinds.All).Where(t => !t.IsAbstract && Implements(t, typeof(Mediator.IQueryHandler<,>)));

    private static IEnumerable<Type> Handlers => TypesOf(Kinds.All).Where(t => !t.IsAbstract && t.GetInterfaces().Any(i =>
        i.IsGenericType && i.GetGenericTypeDefinition().FullName is { } name
        && (name.StartsWith("Mediator.ICommandHandler`", StringComparison.Ordinal)
            || name.StartsWith("Mediator.IQueryHandler`", StringComparison.Ordinal)
            || name.StartsWith("Mediator.INotificationHandler`", StringComparison.Ordinal))));

    [Fact]
    public void Messages_and_handlers_live_where_they_belong()
    {
        var all = TypesOf(Kinds.All).Where(t => !t.IsAbstract).ToList();

        all.Where(t => Implements(t, typeof(Mediator.IQueryHandler<,>)) && Owner(t)!.Kind != Kinds.Infrastructure)
            .Select(t => $"{t.FullName} is a query handler outside a module's Infrastructure")
        .Concat(all.Where(t => Implements(t, typeof(Mediator.ICommandHandler<,>)) && Owner(t)!.Kind != Kinds.Application)
            .Select(t => $"{t.FullName} is a command handler outside a module's Application"))
        .Concat(all.Where(DomainTests.IsMessage).Where(t => Owner(t)!.Kind is not (Kinds.Application or Kinds.Contracts))
            .Select(t => $"{t.FullName} is a message outside a module's Application or Contracts"))
            .ShouldHaveNoViolations("Messages and command handlers live in Application, published ones in Contracts; query handlers live in Infrastructure/Queries.");
    }

    [Fact]
    public void A_query_never_writes()
    {
        string[] writes = ["SaveChanges", "SaveChangesAsync", "ExecuteUpdate", "ExecuteUpdateAsync", "ExecuteDelete", "ExecuteDeleteAsync", "ExecuteSql", "ExecuteSqlAsync"];

        CallsOf(QueryHandlers.SelectMany(WithNested))
            .Where(c => writes.Any(w => c.Method.StartsWith(w + "(", StringComparison.Ordinal)))
            .Select(c => $"{c.Origin.FullName} calls {c.Method}")
            .ShouldHaveNoViolations("A query never writes.");
    }

    [Fact]
    public void A_query_never_loads_an_aggregate()
    {
        DependenciesOf(QueryHandlers.SelectMany(WithNested))
            .Where(d => Owner(d.Target)?.Kind == Kinds.Domain)
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("A query handler projects straight into its Response DTO; it never touches the domain model.");
    }

    [Fact]
    public void Messages_and_responses_carry_no_domain_types()
    {
        var messages = TypesOf(Kinds.Application, Kinds.Contracts).Where(DomainTests.IsMessage).ToList();
        var responses = messages
            .SelectMany(m => m.GetInterfaces().Where(i => i.IsGenericType && i.FullName?.StartsWith("Mediator.", StringComparison.Ordinal) == true))
            .SelectMany(i => i.GetGenericArguments());

        messages.Concat(responses).SelectMany(Unwrap).Distinct()
            .SelectMany(root => Reachable(root).Where(t => Owner(t)?.Kind == Kinds.Domain).Select(t => $"{root.Name} reaches {t.FullName}"))
            .ShouldHaveNoViolations("Messages and DTOs carry the module's wire ids from Contracts, never domain types; the handler converts.");
    }

    [Fact]
    public void Handlers_do_not_log()
    {
        string[] loggers = ["Microsoft.Extensions.Logging.ILogger", "Microsoft.Extensions.Logging.ILogger`1", "Microsoft.Extensions.Logging.ILoggerFactory", "Serilog.ILogger"];

        DependenciesOf(Handlers.SelectMany(WithNested))
            .Where(d => loggers.Contains(d.Target.FullName))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("Handlers don't log; the logging behavior does.");
    }

    [Fact]
    public void Application_and_domain_read_no_raw_claims()
    {
        DependenciesOf(ArchTypesOf(Kinds.Domain, Kinds.Application))
            .Where(d => InNamespace(d.Target, "System.Security.Claims") || InNamespace(d.Target, "System.IdentityModel.Tokens.Jwt"))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("Application and domain code never read raw claims; claims map to policies at the composition root.");
    }

    [Fact]
    public void Integration_events_carry_their_version_in_their_name()
    {
        TypesOf(Kinds.Contracts)
            .Where(t => !t.IsAbstract && typeof(IIntegrationEvent).IsAssignableFrom(t))
            .Where(t => !VersionSuffix().IsMatch(t.Name))
            .Select(t => $"{t.FullName} has no version suffix, such as V1")
            .ShouldHaveNoViolations("An integration event's type name ends in its version; a new shape is a new version.");
    }

    /// <summary>Every type reachable through public properties and constructor parameters.</summary>
    private static IEnumerable<Type> Reachable(Type root)
    {
        var seen = new HashSet<Type>();
        var queue = new Queue<Type>([root]);
        while (queue.Count > 0)
        {
            var type = queue.Dequeue();
            if (!seen.Add(type) || type.Assembly == typeof(object).Assembly)
            {
                continue;
            }

            var next = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.PropertyType)
                .Concat(type.GetConstructors().SelectMany(c => c.GetParameters()).Select(p => p.ParameterType))
                .SelectMany(Unwrap);
            foreach (var t in next)
            {
                queue.Enqueue(t);
            }
        }

        return seen;
    }

    [GeneratedRegex(@"V[1-9][0-9]*$")]
    private static partial Regex VersionSuffix();
}
