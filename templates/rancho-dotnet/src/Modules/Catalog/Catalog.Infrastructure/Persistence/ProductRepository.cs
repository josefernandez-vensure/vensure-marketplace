using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

internal sealed class ProductRepository(CatalogDbContext db) : IProductRepository
{
    public Task<Product?> Find(ProductId id, CancellationToken cancellationToken) =>
        db.Products.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Save(Product root)
    {
        if (db.Entry(root).State == EntityState.Detached)
        {
            db.Products.Add(root);
        }
    }
}
