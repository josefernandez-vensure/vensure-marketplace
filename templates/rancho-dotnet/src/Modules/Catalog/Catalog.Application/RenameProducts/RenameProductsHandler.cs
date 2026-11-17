using Catalog.Domain.Products;
using Platform.Application.Abstractions;

namespace Catalog.Application.RenameProducts;

/// <summary>The rule over several products is the domain service's; the handler converts and translates.</summary>
public sealed class RenameProductsHandler(ProductRenamer renamer) : ICommandHandler<RenameProducts>
{
    public async ValueTask<Result> Handle(RenameProducts command, CancellationToken cancellationToken)
    {
        var names = command.Products.ToDictionary(p => new ProductId(p.ProductId.Value), p => new ProductName(p.Name));
        var outcome = await renamer.RenameAll(names, cancellationToken);

        return outcome switch
        {
            RenameProductsOutcome.Renamed => Result.Success(),
            RenameProductsOutcome.ProductNotFound => CatalogProblems.ProductNotFound.With(),
            RenameProductsOutcome.Discontinued => CatalogProblems.ProductDiscontinued.With(),
        };
    }
}
