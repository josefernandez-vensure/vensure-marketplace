using SharedKernel;

namespace Catalog.Domain.Products;

public interface IProductRepository : IRepository<Product, ProductId>;
