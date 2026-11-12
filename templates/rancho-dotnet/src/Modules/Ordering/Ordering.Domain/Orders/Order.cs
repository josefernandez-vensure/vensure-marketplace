using SharedKernel;

namespace Ordering.Domain.Orders;

/// <summary>
/// An order, and the process that fulfils it: placed <see cref="OrderStatus.Pending"/> while Catalog reserves its stock, then confirmed,
/// or rejected when the stock can't be held. Confirmation is the point of no return; until then the order can be cancelled.
/// </summary>
public sealed class Order : AggregateRoot<OrderId>
{
    private Order(OrderId id, ProductId productId, int quantity, DateTimeOffset placedAt)
    {
        Id = id;
        ProductId = productId;
        Quantity = quantity;
        PlacedAt = placedAt;
    }

    public ProductId ProductId { get; private set; }

    public int Quantity { get; private set; }

    public DateTimeOffset PlacedAt { get; private set; }

    public OrderStatus Status { get; private set; }

    public static Order Place(OrderId id, ProductId productId, int quantity, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        var order = new Order(id, productId, quantity, now) { Status = OrderStatus.Pending };
        order.Touch();
        return order;
    }

    /// <summary>Its stock is held: the order is confirmed, unless it was cancelled or rejected meanwhile and the stock must be released.</summary>
    public ConfirmOutcome Confirm()
    {
        switch (Status)
        {
            case OrderStatus.Pending:
                Status = OrderStatus.Confirmed;
                Touch();
                return ConfirmOutcome.Confirmed;
            case OrderStatus.Confirmed:
                return ConfirmOutcome.Unchanged;
            default:
                return ConfirmOutcome.Abandoned;
        }
    }

    /// <summary>Its stock could not be held.</summary>
    public RejectOutcome Reject()
    {
        if (Status != OrderStatus.Pending)
        {
            return RejectOutcome.Unchanged;
        }

        Status = OrderStatus.Rejected;
        Touch();
        return RejectOutcome.Rejected;
    }

    /// <summary>A pending order can be cancelled; a confirmed one is past the point of no return.</summary>
    public CancelOutcome Cancel()
    {
        switch (Status)
        {
            case OrderStatus.Pending:
                Status = OrderStatus.Cancelled;
                Touch();
                return CancelOutcome.Cancelled;
            case OrderStatus.Confirmed:
                return CancelOutcome.AlreadyConfirmed;
            default:
                return CancelOutcome.Unchanged;
        }
    }
}
