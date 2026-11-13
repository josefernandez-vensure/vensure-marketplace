using Catalog.Application.ListProducts;
using Catalog.Contracts;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using SharedKernel;
using Platform.Application.Abstractions;

namespace Api.Endpoints.Catalog;

public sealed class ListProductsEndpoint : IEndpoint<ListProducts>
{
    public static string Method => HttpMethods.Get;

    public static string Pattern => "/catalog/products";

    public static Delegate Handler => Handle;

    private static async Task<Results<Ok<Page<ProductSummary>>, ProblemHttpResult>> Handle(
        [ClassOf<ProductName>] string? nameStartsWith,
        [Internal] int? pageSize,
        [Internal] string? cursor,
        ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListProducts(nameStartsWith, pageSize ?? Paging.DefaultPageSize, cursor), cancellationToken);
        return result.ToOk();
    }
}
