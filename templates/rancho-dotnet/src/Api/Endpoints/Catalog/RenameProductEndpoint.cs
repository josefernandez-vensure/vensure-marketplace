using Catalog.Application.RenameProduct;
using Catalog.Contracts;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using SharedKernel;

namespace Api.Endpoints.Catalog;

public sealed record RenameProductRequest([ClassOf<ProductName>] string Name);

/// <summary>A write that leaves the resource in place returns its new ETag in the body.</summary>
public sealed record RenameProductResponse([Internal] string ETag);

public sealed class RenameProductEndpoint : IEndpoint<RenameProduct>
{
    public static string Method => HttpMethods.Post;

    public static string Pattern => "/catalog/products/{id:guid}/rename";

    public static Delegate Handler => Handle;

    private static async Task<Results<Ok<RenameProductResponse>, ProblemHttpResult>> Handle([ClassOf<ProductId>] Guid id, RenameProductRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RenameProduct(new ProductId(id), request.Name), cancellationToken);
        return result.ToOk(version => new RenameProductResponse($"\"{version}\""));
    }
}
