using Catalog.Application.GetProduct;
using Catalog.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Queries.GetProduct;

internal sealed class GetProductHandler(CatalogDbContext db)
    : IQueryHandler<Application.GetProduct.GetProduct, ProductResponse?>
{
    public async ValueTask<ProductResponse?> Handle(Application.GetProduct.GetProduct query, CancellationToken cancellationToken)
    {
        var id = query.ProductId.Value;
        var row = await db.Database
            .SqlQuery<ProductRow>($"SELECT Id, Name, IsDiscontinued, Version FROM catalog.Products WHERE Id = {id}")
            .SingleOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new ProductResponse(new Contracts.ProductId(row.Id), row.Name, row.IsDiscontinued, $"\"{row.Version}\"");
    }

    private sealed record ProductRow(Guid Id, string Name, bool IsDiscontinued, long Version);
}
