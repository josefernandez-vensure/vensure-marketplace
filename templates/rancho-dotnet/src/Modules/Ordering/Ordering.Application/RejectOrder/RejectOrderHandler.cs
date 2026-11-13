using Ordering.Domain.Orders;
using Platform.Application.Abstractions;

namespace Ordering.Application.RejectOrder;

public sealed class RejectOrderHandler(IOrderRepository orders) : ICommandHandler<RejectOrder>
{
    public async ValueTask<Result> Handle(RejectOrder command, CancellationToken cancellationToken)
    {
        var order = await orders.Find(new OrderId(command.OrderId.Value), cancellationToken);
        if (order is null)
        {
            return OrderingProblems.OrderNotFound.With();
        }

        var outcome = order.Reject();
        orders.Save(order);

        return outcome switch
        {
            RejectOutcome.Rejected => Result.Success(),
            RejectOutcome.Unchanged => Result.Success(),
        };
    }
}
