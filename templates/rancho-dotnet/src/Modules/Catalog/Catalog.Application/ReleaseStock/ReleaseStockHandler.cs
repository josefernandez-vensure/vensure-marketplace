using Catalog.Domain.Products;
using Platform.Application.Abstractions;

namespace Catalog.Application.ReleaseStock;

/// <summary>Releasing what is not held changes nothing and succeeds, so a repeated compensation is harmless.</summary>
public sealed class ReleaseStockHandler(IProductRepository products) : ICommandHandler<ReleaseStock>
{
    public async ValueTask<Result> Handle(ReleaseStock command, CancellationToken cancellationToken)
    {
        var product = await products.Find(new ProductId(command.ProductId.Value), cancellationToken);
        if (product is null)
        {
            return Result.Success();
        }

        var outcome = product.ReleaseStock(new ReservationId(command.ReservationId.Value));
        products.Save(product);

        return outcome switch
        {
            ReleaseStockOutcome.Released => Result.Success(),
            ReleaseStockOutcome.Unchanged => Result.Success(),
        };
    }
}
