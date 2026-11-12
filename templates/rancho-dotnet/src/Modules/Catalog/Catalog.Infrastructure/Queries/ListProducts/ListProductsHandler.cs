using Catalog.Application.ListProducts;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions;

namespace Catalog.Infrastructure.Queries.ListProducts;

/// <summary>Public because the dispatcher's generated code names it. Its ordering is declared once, as a keyset.</summary>
public sealed class ListProductsHandler(CatalogDbContext db, ICursorCodec cursors)
    : IQueryHandler<Application.ListProducts.ListProducts, Page<ProductSummary>>
{
    private static readonly Keyset<ProductSummaryRow, string> Order = new(r => r.Name, r => r.Id);

    public async ValueTask<Result<Page<ProductSummary>>> Handle(Application.ListProducts.ListProducts query, CancellationToken cancellationToken)
    {
        var rows = db.Set<ProductSummaryRow>().AsNoTracking();
        if (query.NameStartsWith is { Length: > 0 } prefix)
        {
            rows = rows.Where(r => r.Name.StartsWith(prefix));
        }

        var page = await Order.Read(rows, query, cursors).ToListAsync(cancellationToken);
        return Order.Page(page, query, r => new ProductSummary(new Contracts.ProductId(r.Id), r.Name), cursors);
    }
}
