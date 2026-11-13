using Ordering.Domain.Orders;
using Platform.Application.Abstractions;

namespace Ordering.Application.CancelOrder;

public sealed class CancelOrderHandler(IOrderRepository orders) : ICommandHandler<CancelOrder, long>
{
    public async ValueTask<Result<long>> Handle(CancelOrder command, CancellationToken cancellationToken)
    {
        var order = await orders.Find(new OrderId(command.OrderId.Value), cancellationToken);
        if (order is null)
        {
            return OrderingProblems.OrderNotFound.With();
        }

        var outcome = order.Cancel();
        orders.Save(order);

        return outcome switch
        {
            CancelOutcome.Cancelled => order.Version,
            CancelOutcome.Unchanged => order.Version,
            CancelOutcome.AlreadyConfirmed => OrderingProblems.OrderAlreadyConfirmed.With(),
        };
    }
}
