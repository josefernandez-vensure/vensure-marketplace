using Catalog.Contracts;
using Catalog.Contracts.CommandMessages;
using Platform.Application.Abstractions;

namespace Catalog.Application.ReleaseStock;

/// <summary>A compensation: it records the release, and is retried until it succeeds.</summary>
public sealed record ReleaseStock(ReservationId ReservationId, ProductId ProductId) : ICommand
{
    public static Operation Operation { get; } =
        Operation.Command("catalog.release-stock", CatalogPolicies.React, IdempotencyMechanism.Key);
}

public sealed class ReleaseStockReaction : IReaction<ReleaseStockV1, ReleaseStock>
{
    public static ReleaseStock Map(ReleaseStockV1 message) => new(message.ReservationId, message.ProductId);
}
