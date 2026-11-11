using Catalog.Application.GetProduct;
using Catalog.Contracts;
using Mediator;

namespace Api.Endpoints.Catalog;

public sealed class GetProductEndpoint : IEndpoint<GetProduct>
{
    public static string Method => HttpMethods.Get;

    public static string Pattern => "/catalog/products/{id:guid}";

    public static Delegate Handler => Handle;

    private static async Task<IResult> Handle(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProduct(new ProductId(id)), cancellationToken);
        return result.ToOk();
    }
}
