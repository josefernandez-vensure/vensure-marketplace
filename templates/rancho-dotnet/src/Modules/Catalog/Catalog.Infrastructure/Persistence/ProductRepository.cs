using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions;

namespace Catalog.Infrastructure.Persistence;

/// <summary>Loads and saves products within the acting principal's tenant: another tenant's product is never found.</summary>
internal sealed class ProductRepository(CatalogDbContext db, DispatchContext dispatch) : IProductRepository
{
    public const string TenantColumn = "TenantId";

    public Task<Product?> Find(ProductId id, CancellationToken cancellationToken) =>
        dispatch.Tenant is { Value: var tenant }
            ? db.Products.SingleOrDefaultAsync(p => p.Id == id && EF.Property<Guid>(p, TenantColumn) == tenant, cancellationToken)
            : Task.FromResult<Product?>(null);

    public void Save(Product root)
    {
        if (db.Entry(root).State == EntityState.Detached)
        {
            var tenant = dispatch.Tenant ?? throw new InvalidOperationException("A product is created only within a tenant; the dispatch context has none.");
            db.Products.Add(root).Property(TenantColumn).CurrentValue = tenant.Value;
        }
    }
}
