using Platform.Application.Abstractions;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

public sealed class PersistenceTests
{
    [Fact]
    public void Migrations_reference_no_application_code()
    {
        string[] allowed = ["System", "Microsoft.EntityFrameworkCore"];

        OfKind(Kinds.Infrastructure, Kinds.PlatformInfrastructure)
            .SelectMany(a => a.Assembly.GetTypes().Where(InfrastructureTests.IsMigration))
            .SelectMany(migration =>
            {
                var context = migration.GetCustomAttributesData()
                    .FirstOrDefault(a => a.AttributeType.Name == "DbContextAttribute")?.ConstructorArguments[0].Value as Type;
                return DependenciesOf(WithNested(migration))
                    .Where(d => !allowed.Any(ns => InNamespace(d.Target, ns)) && d.Target.FullName != context?.FullName)
                    .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}");
            })
            .ShouldHaveNoViolations("A migration depends on nothing but EF Core, the BCL, and its own DbContext, so it replays the same forever.");
    }

    [Fact]
    public void Only_the_pipeline_touches_the_unit_of_work()
    {
        string[] ports = [typeof(IUnitOfWorkFactory).FullName!, typeof(IUnitOfWork).FullName!, typeof(AttemptIdentity).FullName!, typeof(RecordedResult).FullName!];

        DependenciesOf(Model.Types)
            .Where(d => ports.Contains(d.Target.FullName))
            .Where(d => Owner(d.Origin)?.Kind is not (Kinds.PlatformApplication or Kinds.PlatformInfrastructure or Kinds.Api))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("Idempotency and the unit of work are a pipeline behavior, never a handler's or an endpoint's concern.");
    }

    [Fact]
    public void Published_messages_leave_only_through_the_outbox()
    {
        string[] brokers = ["Azure.Messaging", "MassTransit", "RabbitMQ", "Confluent.Kafka", "NServiceBus", "Amazon.SQS", "Amazon.SimpleNotificationService"];

        var portMethods = TypesOf(Kinds.Application, Kinds.PlatformApplication)
            .Where(t => t.IsInterface && t != typeof(IOutbox))
            .SelectMany(t => t.GetMethods().Where(m => m.GetParameters().Any(p => Unwrap(p.ParameterType).Any(IsPublishedMessage)))
                .Select(m => $"{t.FullName}.{m.Name} takes an integration event or a command message"));

        var sdkUses = DependenciesOf(ArchTypesOf(Kinds.Domain, Kinds.Application, Kinds.Contracts, Kinds.Infrastructure, Kinds.PlatformApplication))
            .Where(d => brokers.Any(ns => InNamespace(d.Target, ns)))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}");

        portMethods.Concat(sdkUses)
            .ShouldHaveNoViolations("An integration event or a command message is written to the outbox with the change; nothing sends it directly.");
    }

    private static bool IsPublishedMessage(Type type) =>
        typeof(IIntegrationEvent).IsAssignableFrom(type) || typeof(ICommandMessage).IsAssignableFrom(type);

    [Fact]
    public void Idempotency_records_live_in_the_database_with_the_change()
    {
        string[] elsewhere = ["Microsoft.Extensions.Caching.Distributed.IDistributedCache", "Microsoft.Extensions.Caching.Memory.IMemoryCache",
            "Microsoft.Extensions.Caching.Hybrid.HybridCache", "StackExchange.Redis.IConnectionMultiplexer", "Microsoft.EntityFrameworkCore.IDbContextFactory`1"];

        var stores = TypesOf(Kinds.PlatformInfrastructure, Kinds.Infrastructure).Where(t => typeof(IUnitOfWork).IsAssignableFrom(t));

        DependenciesOf(stores.SelectMany(WithNested))
            .Where(d => elsewhere.Contains(d.Target.FullName))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}")
            .ShouldHaveNoViolations("The idempotency record commits through the module's own unit of work: never a cache, never another context.");
    }
}
