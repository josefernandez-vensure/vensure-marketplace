namespace Ordering.Domain.Orders;

public enum OrderStatus
{
    /// <summary>In flight: its stock is being reserved. Only a pending order can be cancelled.</summary>
    Pending,
    Confirmed,
    Rejected,
    Cancelled,
}
