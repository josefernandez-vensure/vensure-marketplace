using Catalog.Application.CreateProduct;
using Catalog.Contracts;
using Mediator;

namespace Api.Endpoints.Catalog;

public sealed record CreateProductRequest(Guid Id, string Name);

public sealed class CreateProductEndpoint : IEndpoint<CreateProduct>
{
    public static string Method => HttpMethods.Post;

    public static string Pattern => "/catalog/products";

    public static Delegate Handler => Handle;

    private static async Task<IResult> Handle(CreateProductRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateProduct(new ProductId(request.Id), request.Name), cancellationToken);
        return result.ToCreated(id => $"/catalog/products/{id.Value}");
    }
}
