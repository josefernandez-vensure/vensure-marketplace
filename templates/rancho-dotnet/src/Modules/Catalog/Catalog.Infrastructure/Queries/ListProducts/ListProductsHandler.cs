using Catalog.Application.ListProducts;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions;

namespace Catalog.Infrastructure.Queries.ListProducts;

/// <summary>
/// Public because the dispatcher's generated code names it. Each sort's ordering is declared once, as a keyset; the platform binds a cursor to
/// the sort and the filter that issued it, so a cursor from one keyset never reaches another.
/// </summary>
public sealed class ListProductsHandler(CatalogDbContext db, ICursorCodec cursors, DispatchContext dispatch)
    : IQueryHandler<Application.ListProducts.ListProducts, Page<ProductSummary>>
{
    private static readonly Keyset<ProductSummaryRow, string> ByName = new(r => r.Name, r => r.Id);
    private static readonly Keyset<ProductSummaryRow, int> ByStock = new(r => r.StockOnHand, r => r.Id);

    public async ValueTask<Result<Page<ProductSummary>>> Handle(Application.ListProducts.ListProducts query, CancellationToken cancellationToken)
    {
        var tenant = dispatch.Tenant?.Value;
        var rows = db.Set<ProductSummaryRow>().AsNoTracking().Where(r => r.TenantId == tenant);
        if (query.NameStartsWith is { Length: > 0 } prefix)
        {
            rows = rows.Where(r => r.Name.StartsWith(prefix));
        }

        return query.Sort == ProductSort.StockOnHand
            ? await Read(ByStock, rows, query, cancellationToken)
            : await Read(ByName, rows, query, cancellationToken);
    }

    private async Task<Result<Page<ProductSummary>>> Read<TKey>(
        Keyset<ProductSummaryRow, TKey> order, IQueryable<ProductSummaryRow> rows, Application.ListProducts.ListProducts query, CancellationToken cancellationToken)
    {
        var page = await order.Read(rows, query, cursors).ToListAsync(cancellationToken);
        return order.Page(page, query, r => new ProductSummary(new Contracts.ProductId(r.Id), r.Name), cursors);
    }
}
