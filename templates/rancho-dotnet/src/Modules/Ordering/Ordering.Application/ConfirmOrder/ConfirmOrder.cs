using Catalog.Contracts.IntegrationEvents;
using Ordering.Contracts;
using Platform.Application.Abstractions;

namespace Ordering.Application.ConfirmOrder;

/// <summary>A reaction's command: only the Ordering module's service principal holds its policy.</summary>
public sealed record ConfirmOrder(OrderId OrderId) : ICommand
{
    public static Operation Operation { get; } =
        Operation.Command("ordering.confirm-order", OrderingPolicies.React, IdempotencyMechanism.Key);
}

/// <summary>Catalog's reply that the order's stock is held. The reservation id is the order's id.</summary>
public sealed class ConfirmOrderReaction : IReaction<StockReservedV1, ConfirmOrder>
{
    public static ConfirmOrder Map(StockReservedV1 message) => new(new OrderId(message.ReservationId.Value));
}
