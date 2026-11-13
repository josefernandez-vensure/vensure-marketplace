using Catalog.Contracts;
using Catalog.Contracts.CommandMessages;
using Ordering.Domain.Orders;
using Platform.Application.Abstractions;

namespace Ordering.Application.PlaceOrder;

/// <summary>Catalog owns <see cref="ReserveStockV1"/>; the order asks for its stock by writing that message to its own outbox, with the order.</summary>
public sealed class PlaceOrderHandler(IOrderRepository orders, IOutbox outbox, TimeProvider time) : ICommandHandler<PlaceOrder, Contracts.OrderId>
{
    public async ValueTask<Result<Contracts.OrderId>> Handle(PlaceOrder command, CancellationToken cancellationToken)
    {
        var id = new OrderId(command.OrderId.Value);
        if (await orders.Find(id, cancellationToken) is not null)
        {
            return command.OrderId;
        }

        var order = Order.Place(id, new Domain.Orders.ProductId(command.ProductId.Value), command.Quantity, time.GetUtcNow());
        orders.Save(order);
        outbox.Send(new ReserveStockV1(new ReservationId(id.Value), command.ProductId, command.Quantity));
        return command.OrderId;
    }
}
