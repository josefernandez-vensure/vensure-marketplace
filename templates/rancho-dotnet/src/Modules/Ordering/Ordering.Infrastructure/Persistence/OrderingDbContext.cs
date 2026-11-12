using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Orders;
using Platform.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Persistence;

/// <summary>The module's one DbContext. Public because the dispatcher's generated code constructs its query handlers.</summary>
public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : ModuleDbContext(options)
{
    public const string SchemaName = "ordering";

    public const string ModuleName = "Ordering";

    public override string Module => ModuleName;

    public override string Schema => SchemaName;

    public DbSet<Order> Orders => Set<Order>();

    protected override void ConfigureModule(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);
}
