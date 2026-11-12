using Catalog.Contracts;
using Catalog.Contracts.CommandMessages;
using Ordering.Domain.Orders;
using Platform.Application.Abstractions;

namespace Ordering.Application.ConfirmOrder;

/// <summary>
/// The step after the compensable one. When the order was abandoned while its stock was being reserved, this step fails, and its
/// compensation runs: Catalog is asked to release the stock it holds.
/// </summary>
public sealed class ConfirmOrderHandler(IOrderRepository orders, IOutbox outbox) : ICommandHandler<ConfirmOrder>
{
    public async ValueTask<Result> Handle(ConfirmOrder command, CancellationToken cancellationToken)
    {
        var order = await orders.Find(new OrderId(command.OrderId.Value), cancellationToken);
        if (order is null)
        {
            return OrderingProblems.OrderNotFound.With();
        }

        var outcome = order.Confirm();
        orders.Save(order);

        return outcome switch
        {
            ConfirmOutcome.Confirmed => Result.Success(),
            ConfirmOutcome.Unchanged => Result.Success(),
            ConfirmOutcome.Abandoned => Compensate(order),
        };
    }

    private Result Compensate(Order order)
    {
        outbox.Send(new ReleaseStockV1(new ReservationId(order.Id.Value), new Catalog.Contracts.ProductId(order.ProductId.Value)));
        return Result.Success();
    }
}
