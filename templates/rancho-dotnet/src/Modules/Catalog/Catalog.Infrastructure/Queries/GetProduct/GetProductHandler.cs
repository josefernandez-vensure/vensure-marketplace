using Catalog.Application;
using Catalog.Application.GetProduct;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions;

namespace Catalog.Infrastructure.Queries.GetProduct;

/// <summary>Public because the dispatcher's generated code names it. It reads with SQL straight into the DTO.</summary>
public sealed class GetProductHandler(CatalogDbContext db) : IQueryHandler<Application.GetProduct.GetProduct, ProductResponse>
{
    public async ValueTask<Result<ProductResponse>> Handle(Application.GetProduct.GetProduct query, CancellationToken cancellationToken)
    {
        var id = query.ProductId.Value;
        var row = await db.Database
            .SqlQuery<ProductRow>($"SELECT Id, Name, IsDiscontinued, Version FROM catalog.Products WHERE Id = {id}")
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return CatalogProblems.ProductNotFound.With();
        }

        return new ProductResponse(new Contracts.ProductId(row.Id), row.Name, row.IsDiscontinued, $"\"{row.Version}\"");
    }

    private sealed record ProductRow(Guid Id, string Name, bool IsDiscontinued, long Version);
}
