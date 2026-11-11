using Catalog.Domain.Products;
using Platform.Application.Abstractions;

namespace Catalog.Application.RenameProduct;

public sealed class RenameProductHandler(IProductRepository products) : ICommandHandler<RenameProduct, long>
{
    public async ValueTask<Result<long>> Handle(RenameProduct command, CancellationToken cancellationToken)
    {
        var product = await products.Find(new ProductId(command.ProductId.Value), cancellationToken);
        if (product is null)
        {
            return CatalogProblems.ProductNotFound.With();
        }

        var outcome = product.Rename(new ProductName(command.Name));
        if (outcome == RenameOutcome.Renamed)
        {
            await products.Save(product, cancellationToken);
        }

        // A switch expression over the outcome with no discard arm: a new outcome fails the build (CS8509).
        return outcome switch
        {
            RenameOutcome.Renamed => product.Version,
            RenameOutcome.Unchanged => product.Version,
            RenameOutcome.Discontinued => CatalogProblems.ProductDiscontinued.With(),
        };
    }
}
