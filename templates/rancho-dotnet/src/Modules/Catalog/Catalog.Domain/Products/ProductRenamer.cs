using SharedKernel;

namespace Catalog.Domain.Products;

/// <summary>Corrects the names of several products at once: every product is renamed, or none is.</summary>
public sealed class ProductRenamer(IProductRepository products) : IDomainService
{
    public async Task<RenameProductsOutcome> RenameAll(IReadOnlyDictionary<ProductId, ProductName> names, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(names);
        var found = await products.FindAll([.. names.Keys], cancellationToken);
        if (found.Count != names.Count)
        {
            return RenameProductsOutcome.ProductNotFound;
        }

        if (found.Any(p => p.IsDiscontinued))
        {
            return RenameProductsOutcome.Discontinued;
        }

        foreach (var product in found)
        {
            // Not discontinued, so each is renamed or already carries the name.
            _ = product.Rename(names[product.Id]);
            products.Save(product);
        }

        return RenameProductsOutcome.Renamed;
    }
}
