using Catalog.Domain.Products;
using Mediator;

namespace Catalog.Application.CreateProduct;

public sealed class CreateProductHandler(IProductRepository products, TimeProvider time)
    : ICommandHandler<CreateProduct, Contracts.ProductId>
{
    public async ValueTask<Contracts.ProductId> Handle(CreateProduct command, CancellationToken cancellationToken)
    {
        var product = Product.Create(ProductId.New(), new ProductName(command.Name), time.GetUtcNow());
        await products.Save(product, cancellationToken);
        return new Contracts.ProductId(product.Id.Value);
    }
}
