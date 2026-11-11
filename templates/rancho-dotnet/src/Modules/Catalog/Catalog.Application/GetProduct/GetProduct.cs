using Catalog.Contracts;
using Platform.Application.Abstractions;

namespace Catalog.Application.GetProduct;

public sealed record GetProduct(ProductId ProductId) : IQuery<ProductResponse>
{
    public static Operation Operation { get; } = Operation.Query("catalog.get-product", CatalogPolicies.Read);
}

public sealed record ProductResponse(ProductId Id, string Name, bool IsDiscontinued, int StockOnHand, string ETag);
