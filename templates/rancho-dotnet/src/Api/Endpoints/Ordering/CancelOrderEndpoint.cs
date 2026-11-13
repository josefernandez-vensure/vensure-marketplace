using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.CancelOrder;
using Ordering.Contracts;
using Platform.Application.Abstractions;
using SharedKernel;

namespace Api.Endpoints.Ordering;

/// <summary>A write that leaves the resource in place returns its new ETag in the body.</summary>
public sealed record CancelOrderResponse([Internal] string Etag);

public sealed class CancelOrderEndpoint : IEndpoint<CancelOrder>
{
    public static string Method => HttpMethods.Post;

    public static string Pattern => "/ordering/orders/{id:guid}/cancel";

    public static Delegate Handler => Handle;

    private static async Task<Results<Ok<CancelOrderResponse>, ProblemHttpResult>> Handle(
        [ClassOf<OrderId>] Guid id,
        [FromHeader(Name = "If-Match"), Internal] string? ifMatch,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelOrder(new OrderId(id), ExpectedVersion.FromIfMatch(ifMatch)), cancellationToken);
        return result.ToOk(version => new CancelOrderResponse($"\"{version}\""));
    }
}
