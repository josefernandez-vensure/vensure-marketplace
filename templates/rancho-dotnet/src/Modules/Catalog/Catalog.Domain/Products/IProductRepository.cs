using SharedKernel;

namespace Catalog.Domain.Products;

public interface IProductRepository : IRepository<Product, ProductId>
{
    /// <summary>The products with these ids that exist, in no particular order; a missing one is simply absent.</summary>
    Task<IReadOnlyList<Product>> FindAll(IReadOnlyCollection<ProductId> ids, CancellationToken cancellationToken);
}
