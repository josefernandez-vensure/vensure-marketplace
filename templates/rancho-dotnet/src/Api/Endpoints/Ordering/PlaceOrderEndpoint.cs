using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Ordering.Application.PlaceOrder;
using Ordering.Contracts;
using SharedKernel;
using ProductId = Catalog.Contracts.ProductId;

namespace Api.Endpoints.Ordering;

public sealed record PlaceOrderRequest([ClassOf<OrderId>] Guid Id, [ClassOf<ProductId>] Guid ProductId, [Internal] int Quantity);

/// <summary>The order is placed pending; reading it shows when its stock is reserved and it is confirmed or rejected.</summary>
public sealed class PlaceOrderEndpoint : IEndpoint<PlaceOrder>
{
    public static string Method => HttpMethods.Post;

    public static string Pattern => "/ordering/orders";

    public static Delegate Handler => Handle;

    private static async Task<Results<Created, ProblemHttpResult>> Handle(PlaceOrderRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new PlaceOrder(new OrderId(request.Id), new ProductId(request.ProductId), request.Quantity), cancellationToken);
        return result.ToCreated(id => $"/ordering/orders/{id.Value}");
    }
}
