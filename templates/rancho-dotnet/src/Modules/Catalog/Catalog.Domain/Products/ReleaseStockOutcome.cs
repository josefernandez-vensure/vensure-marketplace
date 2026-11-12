namespace Catalog.Domain.Products;

public enum ReleaseStockOutcome
{
    Released,

    /// <summary>Nothing is held under this reservation: it was released already, or never held.</summary>
    Unchanged,
}
