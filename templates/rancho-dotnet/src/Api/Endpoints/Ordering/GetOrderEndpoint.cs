using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Ordering.Application.GetOrder;
using Ordering.Contracts;
using SharedKernel;

namespace Api.Endpoints.Ordering;

public sealed class GetOrderEndpoint : IEndpoint<GetOrder>
{
    public static string Method => HttpMethods.Get;

    public static string Pattern => "/ordering/orders/{id:guid}";

    public static Delegate Handler => Handle;

    private static async Task<Results<Ok<OrderResponse>, ProblemHttpResult>> Handle([ClassOf<OrderId>] Guid id, HttpResponse response, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOrder(new OrderId(id)), cancellationToken);
        if (result.IsSuccess)
        {
            // A read returns the ETag the client sends back as If-Match.
            response.Headers.ETag = result.Value.Etag;
        }

        return result.ToOk();
    }
}
