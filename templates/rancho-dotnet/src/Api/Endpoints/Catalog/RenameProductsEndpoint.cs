using System.ComponentModel.DataAnnotations;
using Catalog.Application.RenameProducts;
using Catalog.Contracts;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Platform.Application.Abstractions;
using SharedKernel;

namespace Api.Endpoints.Catalog;

public sealed record RenameProductsRequest([property: MaxLength(RenameProducts.MaxProducts)] IReadOnlyList<ProductRenameRequest> Products);

public sealed record ProductRenameRequest([ClassOf<ProductId>] Guid Id, [ClassOf<ProductName>] string Name);

/// <summary>Several products in one request, renamed together or not at all. No If-Match: it would name one resource.</summary>
public sealed class RenameProductsEndpoint : IEndpoint<RenameProducts>
{
    public static string Method => HttpMethods.Post;

    public static string Pattern => "/catalog/products/rename";

    public static Delegate Handler => Handle;

    private static async Task<Results<NoContent, ProblemHttpResult>> Handle(RenameProductsRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var products = request.Products.Select(p => new ProductRename(new ProductId(p.Id), p.Name)).ToList();
        var result = await sender.Send(new RenameProducts(products), cancellationToken);
        return result.ToNoContent();
    }
}
