namespace Ordering.Domain.Orders;

public enum ConfirmOutcome
{
    Confirmed,
    Unchanged,

    /// <summary>The order was cancelled or rejected while its stock was being reserved: the reservation must be released.</summary>
    Abandoned,
}
