using Catalog.Contracts;
using Platform.Application.Abstractions;

namespace Catalog.Application.RenameProduct;

/// <summary>Returns the product's new version, which the endpoint sends back as its ETag.</summary>
public sealed record RenameProduct(ProductId ProductId, string Name) : ICommand<long>
{
    public static Operation Operation { get; } =
        Operation.Command("catalog.rename-product", CatalogPolicies.Write, IdempotencyMechanism.Natural);
}
