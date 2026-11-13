using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Platform.Infrastructure.Persistence;
using SharedKernel;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

/// <summary>Each module's EF model, built without a database, read for what the build can't see.</summary>
public sealed class ModelTests
{
    private static readonly Lazy<List<(ProductionAssembly Assembly, ModuleDbContext Context, IModel Model)>> Contexts = new(Build);

    [Fact]
    public void Each_module_has_one_context_on_its_own_schema()
    {
        var violations = new List<string>();
        foreach (var assembly in OfKind(Kinds.Infrastructure))
        {
            var count = Contexts.Value.Count(c => c.Assembly == assembly);
            if (count != 1)
            {
                violations.Add($"{assembly.Name} declares {count} module contexts; it needs exactly one");
            }
        }

        foreach (var (assembly, context, model) in Contexts.Value)
        {
            if (context.Module != assembly.Module)
            {
                violations.Add($"{context.GetType().Name}.Module is '{context.Module}', but its assembly belongs to '{assembly.Module}'");
            }

            foreach (var entity in model.GetEntityTypes())
            {
                var schema = entity.GetSchema() ?? entity.GetViewSchema();
                if (schema != context.Schema)
                {
                    violations.Add($"{context.GetType().Name} maps {entity.DisplayName()} to schema '{schema}', not '{context.Schema}'");
                }

                if (Owner(entity.ClrType) is { Module: { } other } && other != assembly.Module)
                {
                    violations.Add($"{context.GetType().Name} maps {entity.ClrType.FullName}, which module {other} declares");
                }
            }
        }

        violations.ShouldHaveNoViolations("One DbContext per module, on the module's own schema, mapping only the module's own types.");
    }

    [Fact]
    public void Mapped_domain_types_have_no_public_setter_or_parameterless_constructor()
    {
        Contexts.Value
            .SelectMany(c => c.Model.GetEntityTypes().Select(e => e.ClrType))
            .Where(t => Owner(t)?.Kind == Kinds.Domain)
            .Distinct()
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.SetMethod?.IsPublic == true).Select(p => $"{t.Name}.{p.Name} has a public setter")
                .Concat(t.GetConstructor(Type.EmptyTypes) is { IsPublic: true } ? [$"{t.Name} has a public parameterless constructor"] : []))
            .ShouldHaveNoViolations("A domain type is valid at every instant; EF binds through private members, never public setters.");
    }

    [Fact]
    public void No_navigation_leads_from_one_aggregate_to_another()
    {
        Contexts.Value
            .SelectMany(c => c.Model.GetEntityTypes())
            .Where(e => typeof(IAggregateRoot).IsAssignableFrom(e.ClrType))
            .SelectMany(e => e.GetNavigations().Cast<INavigationBase>().Concat(e.GetSkipNavigations())
                .Where(n => typeof(IAggregateRoot).IsAssignableFrom(n.TargetEntityType.ClrType) && n.TargetEntityType.ClrType != e.ClrType)
                .Select(n => $"{e.ClrType.Name}.{n.Name} navigates to {n.TargetEntityType.ClrType.Name}"))
            .ShouldHaveNoViolations("An aggregate holds another aggregate's id, never a navigation to it.");
    }

    [Fact]
    public void Domain_events_carry_ids_and_values_never_entities()
    {
        var entities = Contexts.Value.SelectMany(c => c.Model.GetEntityTypes()).Where(e => !e.IsOwned()).Select(e => e.ClrType).ToHashSet();

        TypesOf(Kinds.Domain)
            .Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && !t.IsInterface)
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => Unwrap(p.PropertyType).Any(entities.Contains))
                .Select(p => $"{t.Name}.{p.Name} carries an entity"))
            .ShouldHaveNoViolations("A domain event carries ids and values; it goes to the outbox and must not drag an entity graph with it.");
    }

    [Fact]
    public void Read_types_are_keyless()
    {
        Contexts.Value
            .SelectMany(c => c.Model.GetEntityTypes())
            .Where(e => !e.IsOwned() && !e.HasSharedClrType && e.FindPrimaryKey() is not null)
            .Where(e => Owner(e.ClrType)?.Kind is not (Kinds.Domain or Kinds.PlatformInfrastructure))
            .Select(e => $"{e.ClrType.FullName} has a key")
            .ShouldHaveNoViolations("A type mapped for reading is keyless, so a query never tracks what it reads.");
    }

    [Fact]
    public void Every_module_model_carries_the_pipelines_tables()
    {
        var violations = new List<string>();
        foreach (var (_, context, model) in Contexts.Value)
        {
            var name = context.GetType().Name;
            var idempotency = model.FindEntityType(typeof(IdempotencyRecord));
            var identity = new[] { nameof(IdempotencyRecord.PrincipalId), nameof(IdempotencyRecord.OperationId), nameof(IdempotencyRecord.Key) };
            if (idempotency?.GetIndexes().Any(i => i.IsUnique && i.GetFilter() is null && i.Properties.Select(p => p.Name).SequenceEqual(identity)) != true)
            {
                violations.Add($"{name}: no unique, unfiltered index on the attempt identity (principal, operation, key)");
            }

            var outbox = model.FindEntityType(typeof(OutboxMessage));
            if (outbox?.FindProperty(nameof(OutboxMessage.TraceParent)) is null || outbox.FindProperty(nameof(OutboxMessage.TraceState)) is null)
            {
                violations.Add($"{name}: the outbox doesn't store the writing transaction's trace context");
            }

            if (model.FindEntityType(typeof(AuditRecord)) is null)
            {
                violations.Add($"{name}: no audit trail");
            }
        }

        violations.ShouldHaveNoViolations("Every module commits its outbox rows, idempotency records and audit records with its own changes.");
    }

    [Fact]
    public void Every_model_change_has_its_migration()
    {
        Contexts.Value
            .Where(c => c.Context.Database.HasPendingModelChanges())
            .Select(c => $"{c.Context.GetType().Name} has model changes no migration captures; run dotnet ef migrations add")
            .ShouldHaveNoViolations("Every schema change is a committed migration.");
    }

    private static List<(ProductionAssembly, ModuleDbContext, IModel)> Build() =>
        OfKind(Kinds.Infrastructure)
            .SelectMany(a => a.Assembly.GetTypes().Where(t => t is { IsAbstract: false } && t.IsSubclassOf(typeof(ModuleDbContext))).Select(t => (a, t)))
            .Select(x =>
            {
                var builder = (DbContextOptionsBuilder)Activator.CreateInstance(typeof(DbContextOptionsBuilder<>).MakeGenericType(x.t))!;
                builder.UseSqlServer("Server=model-only;Database=model-only");
                var context = (ModuleDbContext)Activator.CreateInstance(x.t, builder.Options)!;
                return (x.a, context, context.GetService<IDesignTimeModel>().Model);
            })
            .ToList();
}
