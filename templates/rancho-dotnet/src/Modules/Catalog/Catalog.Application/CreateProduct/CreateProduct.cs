using Catalog.Contracts;
using Platform.Application.Abstractions;

namespace Catalog.Application.CreateProduct;

/// <summary>The client supplies the id, so a repeat finds the product it already created.</summary>
public sealed record CreateProduct(ProductId ProductId, string Name) : ICommand<ProductId>
{
    public static Operation Operation { get; } =
        Operation.Command("catalog.create-product", CatalogPolicies.Write, IdempotencyMechanism.ClientSuppliedId);
}
