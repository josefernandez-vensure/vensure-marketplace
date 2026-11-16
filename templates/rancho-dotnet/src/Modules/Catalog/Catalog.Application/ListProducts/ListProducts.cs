using System.Text.Json.Serialization;
using Catalog.Contracts;
using Platform.Application.Abstractions;
using SharedKernel;

namespace Catalog.Application.ListProducts;

/// <summary>
/// Products by name or by stock on hand, a page at a time. The paging behavior checks the page size and the cursor, which is bound to the
/// sort and the filter it was issued under.
/// </summary>
public sealed record ListProducts(string? NameStartsWith, ProductSort Sort = ProductSort.Name, int PageSize = Paging.DefaultPageSize, string? Cursor = null)
    : IPagedQuery<ProductSummary>
{
    public static int MaxPageSize => 50;

    public static Operation Operation { get; } = Operation.Query("catalog.list-products", CatalogPolicies.Read);
}

/// <summary>Each sort is a keyset of its own in the handler.</summary>
[Internal]
[JsonConverter(typeof(JsonStringEnumConverter<ProductSort>))]
public enum ProductSort
{
    Name,

    /// <summary>Lowest stock first, for restocking.</summary>
    StockOnHand,
}

public sealed record ProductSummary(ProductId Id, [ClassOf<ProductName>] string Name);
