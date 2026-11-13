namespace Catalog.Domain.Products;

/// <summary>Stock held for a process in flight. Releasing it records that it was released; it is never deleted.</summary>
public sealed class StockReservation
{
    private StockReservation(ReservationId id, int quantity)
    {
        Id = id;
        Quantity = quantity;
    }

    public ReservationId Id { get; private set; }

    public int Quantity { get; private set; }

    public bool IsReleased { get; private set; }

    internal static StockReservation Hold(ReservationId id, int quantity) => new(id, quantity);

    internal void Release() => IsReleased = true;
}
