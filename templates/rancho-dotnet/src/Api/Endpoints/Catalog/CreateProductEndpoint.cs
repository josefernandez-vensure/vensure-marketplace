using Catalog.Application.CreateProduct;
using Catalog.Contracts;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using SharedKernel;

namespace Api.Endpoints.Catalog;

public sealed record CreateProductRequest([ClassOf<ProductId>] Guid Id, [ClassOf<ProductName>] string Name);

public sealed class CreateProductEndpoint : IEndpoint<CreateProduct>
{
    public static string Method => HttpMethods.Post;

    public static string Pattern => "/catalog/products";

    public static Delegate Handler => Handle;

    private static async Task<Results<Created, ProblemHttpResult>> Handle(CreateProductRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateProduct(new ProductId(request.Id), request.Name), cancellationToken);
        return result.ToCreated(id => $"/catalog/products/{id.Value}");
    }
}
