using Ordering.Contracts;
using Platform.Application.Abstractions;

namespace Ordering.Application.CancelOrder;

/// <summary>Cancelling twice changes nothing more. A stock reservation still in flight is released when its reply arrives.</summary>
public sealed record CancelOrder(OrderId OrderId, ExpectedVersion ExpectedVersion) : ICommand<long>, IVersionedCommand
{
    public static Operation Operation { get; } =
        Operation.Command("ordering.cancel-order", OrderingPolicies.Write, IdempotencyMechanism.Natural);
}
