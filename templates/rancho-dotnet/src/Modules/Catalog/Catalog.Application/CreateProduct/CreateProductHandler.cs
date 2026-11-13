using Catalog.Domain.Products;
using Platform.Application.Abstractions;

namespace Catalog.Application.CreateProduct;

public sealed class CreateProductHandler(IProductRepository products, TimeProvider time)
    : ICommandHandler<CreateProduct, Contracts.ProductId>
{
    public async ValueTask<Result<Contracts.ProductId>> Handle(CreateProduct command, CancellationToken cancellationToken)
    {
        var id = new ProductId(command.ProductId.Value);
        if (await products.Find(id, cancellationToken) is not null)
        {
            return command.CreateOnly ? PlatformProblems.PreconditionFailed.With() : command.ProductId;
        }

        products.Save(Product.Create(id, new ProductName(command.Name), time.GetUtcNow()));
        return command.ProductId;
    }
}
