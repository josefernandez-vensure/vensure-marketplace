using Catalog.Application.ReceiveStock;
using Catalog.Contracts;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using SharedKernel;

namespace Api.Endpoints.Catalog;

public sealed record ReceiveStockRequest([Internal] int Quantity);

public sealed record ReceiveStockResponse([Internal] string ETag);

/// <summary>The client sends an Idempotency-Key header; the pipeline reads it from the dispatch context.</summary>
public sealed class ReceiveStockEndpoint : IEndpoint<ReceiveStock>
{
    public static string Method => HttpMethods.Post;

    public static string Pattern => "/catalog/products/{id:guid}/stock-receipts";

    public static Delegate Handler => Handle;

    private static async Task<Results<Ok<ReceiveStockResponse>, ProblemHttpResult>> Handle([ClassOf<ProductId>] Guid id, ReceiveStockRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ReceiveStock(new ProductId(id), request.Quantity), cancellationToken);
        return result.ToOk(version => new ReceiveStockResponse($"\"{version}\""));
    }
}
