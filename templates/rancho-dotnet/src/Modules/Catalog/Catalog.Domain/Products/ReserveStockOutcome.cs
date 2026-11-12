namespace Catalog.Domain.Products;

public enum ReserveStockOutcome
{
    Reserved,

    /// <summary>This reservation is already held: reserving it again changes nothing.</summary>
    AlreadyReserved,
    InsufficientStock,
    Discontinued,
}
