using SharedKernel;

namespace Catalog.Domain.Products;

public sealed class Product : AggregateRoot<ProductId>
{
    private readonly List<StockReservation> _reservations = [];

    private Product(ProductId id, ProductName name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
    }

    public ProductName Name { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsDiscontinued { get; private set; }

    public int StockOnHand { get; private set; }

    /// <summary>Every reservation ever held, released or not.</summary>
    public IReadOnlyCollection<StockReservation> Reservations => _reservations;

    /// <summary>Stock on hand that no reservation holds. A process in flight keeps its stock from every other order.</summary>
    public int Available => StockOnHand - _reservations.Where(r => !r.IsReleased).Sum(r => r.Quantity);

    public static Product Create(ProductId id, ProductName name, DateTimeOffset now)
    {
        var product = new Product(id, name, now);
        product.Raise(new ProductCreated(id, now));
        return product;
    }

    public RenameOutcome Rename(ProductName name)
    {
        if (IsDiscontinued)
        {
            return RenameOutcome.Discontinued;
        }

        if (name == Name)
        {
            return RenameOutcome.Unchanged;
        }

        Name = name;
        Touch();
        return RenameOutcome.Renamed;
    }

    /// <summary>Not naturally idempotent: receiving twice doubles the stock. Its command declares an idempotency key.</summary>
    public ReceiveStockOutcome ReceiveStock(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        if (IsDiscontinued)
        {
            return ReceiveStockOutcome.Discontinued;
        }

        StockOnHand += quantity;
        Touch();
        return ReceiveStockOutcome.Received;
    }

    /// <summary>The compensable step of an order: holds stock until the order is confirmed, or releases it if the order is abandoned.</summary>
    public ReserveStockOutcome ReserveStock(ReservationId reservation, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        if (_reservations.Any(r => r.Id == reservation))
        {
            return ReserveStockOutcome.AlreadyReserved;
        }

        if (IsDiscontinued)
        {
            return ReserveStockOutcome.Discontinued;
        }

        if (Available < quantity)
        {
            return ReserveStockOutcome.InsufficientStock;
        }

        _reservations.Add(StockReservation.Hold(reservation, quantity));
        Touch();
        return ReserveStockOutcome.Reserved;
    }

    /// <summary>The compensation of <see cref="ReserveStock"/>: idempotent, and it records the release rather than deleting the reservation.</summary>
    public ReleaseStockOutcome ReleaseStock(ReservationId reservation)
    {
        var held = _reservations.SingleOrDefault(r => r.Id == reservation && !r.IsReleased);
        if (held is null)
        {
            return ReleaseStockOutcome.Unchanged;
        }

        held.Release();
        Touch();
        return ReleaseStockOutcome.Released;
    }

    public void Discontinue()
    {
        if (IsDiscontinued)
        {
            return;
        }

        IsDiscontinued = true;
        Touch();
    }
}
