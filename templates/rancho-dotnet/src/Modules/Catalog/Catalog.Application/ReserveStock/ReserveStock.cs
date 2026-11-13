using Catalog.Contracts;
using Catalog.Contracts.CommandMessages;
using Platform.Application.Abstractions;

namespace Catalog.Application.ReserveStock;

/// <summary>A reaction's command: only the Catalog module's service principal holds its policy, so no caller over HTTP reaches it.</summary>
public sealed record ReserveStock(ReservationId ReservationId, ProductId ProductId, int Quantity) : ICommand
{
    public static Operation Operation { get; } =
        Operation.Command("catalog.reserve-stock", CatalogPolicies.React, IdempotencyMechanism.Key);
}

/// <summary>Catalog owns <see cref="ReserveStockV1"/>, so Catalog alone reacts to it.</summary>
public sealed class ReserveStockReaction : IReaction<ReserveStockV1, ReserveStock>
{
    public static ReserveStock Map(ReserveStockV1 message) => new(message.ReservationId, message.ProductId, message.Quantity);
}
