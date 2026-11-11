using Catalog.Contracts;
using Platform.Application.Abstractions;

namespace Catalog.Application.ReceiveStock;

/// <summary>Repeating it would receive the stock twice, so the client sends an Idempotency-Key and a retry replays.</summary>
public sealed record ReceiveStock(ProductId ProductId, int Quantity) : ICommand<long>
{
    public static Operation Operation { get; } =
        Operation.Command("catalog.receive-stock", CatalogPolicies.Write, IdempotencyMechanism.Key);
}
