using Catalog.Domain.Products;
using Platform.Application.Abstractions;

namespace Catalog.Application.ReceiveStock;

public sealed class ReceiveStockHandler(IProductRepository products) : ICommandHandler<ReceiveStock, long>
{
    public async ValueTask<Result<long>> Handle(ReceiveStock command, CancellationToken cancellationToken)
    {
        var product = await products.Find(new ProductId(command.ProductId.Value), cancellationToken);
        if (product is null)
        {
            return CatalogProblems.ProductNotFound.With();
        }

        var outcome = product.ReceiveStock(command.Quantity);
        products.Save(product);

        return outcome switch
        {
            ReceiveStockOutcome.Received => product.Version,
            ReceiveStockOutcome.Discontinued => CatalogProblems.ProductDiscontinued.With(),
        };
    }
}
