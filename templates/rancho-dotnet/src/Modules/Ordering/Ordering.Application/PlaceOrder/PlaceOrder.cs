using Ordering.Contracts;
using Platform.Application.Abstractions;
using ProductId = Catalog.Contracts.ProductId;

namespace Ordering.Application.PlaceOrder;

/// <summary>
/// Starts the order's saga: the order is placed pending, and Catalog is asked to reserve its stock. The client supplies the id,
/// so a repeat finds the order it already placed and asks for nothing again.
/// </summary>
public sealed record PlaceOrder(OrderId OrderId, ProductId ProductId, int Quantity) : ICommand<OrderId>
{
    public static Operation Operation { get; } =
        Operation.Command("ordering.place-order", OrderingPolicies.Write, IdempotencyMechanism.ClientSuppliedId);
}
