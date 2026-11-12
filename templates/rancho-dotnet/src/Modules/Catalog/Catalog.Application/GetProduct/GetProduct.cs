using Catalog.Contracts;
using Platform.Application.Abstractions;
using SharedKernel;

namespace Catalog.Application.GetProduct;

public sealed record GetProduct(ProductId ProductId) : IQuery<ProductResponse>
{
    public static Operation Operation { get; } = Operation.Query("catalog.get-product", CatalogPolicies.Read);
}

public sealed record ProductResponse(
    ProductId Id,
    [ClassOf<ProductName>] string Name,
    [Public] bool IsDiscontinued,
    [Internal] int StockOnHand,
    [Internal] string ETag);
