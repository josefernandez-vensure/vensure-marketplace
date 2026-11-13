using Catalog.Application.GetProduct;
using Catalog.Contracts;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using SharedKernel;

namespace Api.Endpoints.Catalog;

public sealed class GetProductEndpoint : IEndpoint<GetProduct>
{
    public static string Method => HttpMethods.Get;

    public static string Pattern => "/catalog/products/{id:guid}";

    public static Delegate Handler => Handle;

    private static async Task<Results<Ok<ProductResponse>, ProblemHttpResult>> Handle([ClassOf<ProductId>] Guid id, HttpResponse response, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProduct(new ProductId(id)), cancellationToken);
        if (result.IsSuccess)
        {
            // A read returns the ETag the client sends back as If-Match.
            response.Headers.ETag = result.Value.Etag;
        }

        return result.ToOk();
    }
}
