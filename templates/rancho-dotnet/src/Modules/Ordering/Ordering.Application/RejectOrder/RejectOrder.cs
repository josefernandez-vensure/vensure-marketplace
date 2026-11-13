using Catalog.Contracts.IntegrationEvents;
using Ordering.Contracts;
using Platform.Application.Abstractions;

namespace Ordering.Application.RejectOrder;

/// <summary>A reaction's command: only the Ordering module's service principal holds its policy.</summary>
public sealed record RejectOrder(OrderId OrderId) : ICommand
{
    public static Operation Operation { get; } =
        Operation.Command("ordering.reject-order", OrderingPolicies.React, IdempotencyMechanism.Key);
}

/// <summary>Catalog's reply that the order's stock could not be held.</summary>
public sealed class RejectOrderReaction : IReaction<StockReservationFailedV1, RejectOrder>
{
    public static RejectOrder Map(StockReservationFailedV1 message) => new(new OrderId(message.ReservationId.Value));
}
