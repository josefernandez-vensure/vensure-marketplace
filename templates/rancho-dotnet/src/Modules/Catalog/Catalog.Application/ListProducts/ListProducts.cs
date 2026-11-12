using Catalog.Contracts;
using Platform.Application.Abstractions;

namespace Catalog.Application.ListProducts;

/// <summary>Products by name, a page at a time. The paging behavior checks the page size and the cursor.</summary>
public sealed record ListProducts(string? NameStartsWith, int PageSize = Paging.DefaultPageSize, string? Cursor = null) : IPagedQuery<ProductSummary>
{
    public static int MaxPageSize => 50;

    public static Operation Operation { get; } = Operation.Query("catalog.list-products", CatalogPolicies.Read);
}

public sealed record ProductSummary(ProductId Id, string Name);
