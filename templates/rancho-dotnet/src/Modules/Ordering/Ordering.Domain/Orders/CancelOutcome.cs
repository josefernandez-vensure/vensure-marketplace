namespace Ordering.Domain.Orders;

public enum CancelOutcome
{
    Cancelled,
    Unchanged,
    AlreadyConfirmed,
}
