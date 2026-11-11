using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

internal sealed class ProductRepository(CatalogDbContext db) : IProductRepository
{
    public Task<Product?> Find(ProductId id, CancellationToken cancellationToken) =>
        db.Products.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task Save(Product root, CancellationToken cancellationToken)
    {
        if (db.Entry(root).State == EntityState.Detached)
        {
            db.Products.Add(root);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
