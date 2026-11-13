using Ordering.Contracts;
using Platform.Application.Abstractions;
using SharedKernel;
using ProductId = Catalog.Contracts.ProductId;

namespace Ordering.Application.GetOrder;

public sealed record GetOrder(OrderId OrderId) : IQuery<OrderResponse>
{
    public static Operation Operation { get; } = Operation.Query("ordering.get-order", OrderingPolicies.Read);
}

/// <summary>While its stock is being reserved, an order is <c>Pending</c>, and a read may not show the reply yet.</summary>
public sealed record OrderResponse(
    OrderId Id,
    ProductId ProductId,
    [Internal] int Quantity,
    [Internal] string Status,
    [Internal] string Etag);
