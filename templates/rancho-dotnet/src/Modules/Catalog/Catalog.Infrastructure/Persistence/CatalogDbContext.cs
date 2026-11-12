using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Platform.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence;

/// <summary>The module's one DbContext. Public because the dispatcher's generated code constructs its query handlers.</summary>
public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : ModuleDbContext(options)
{
    public const string SchemaName = "catalog";

    public const string ModuleName = "Catalog";

    public override string Module => ModuleName;

    public override string Schema => SchemaName;

    public DbSet<Product> Products => Set<Product>();

    protected override void ConfigureModule(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
}
