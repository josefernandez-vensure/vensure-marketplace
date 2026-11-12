using Catalog.Domain.Products;
using Shouldly;

namespace Domain.Tests.Catalog;

public sealed class ProductTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_RaisesProductCreated_WithItsIdAndTime()
    {
        var id = ProductId.New();

        var product = Product.Create(id, new ProductName("Lamp"), Now);

        product.DomainEvents.ShouldHaveSingleItem().ShouldBe(new ProductCreated(id, Now));
        product.Version.ShouldBe(1);
    }

    [Fact]
    public void Rename_ReturnsRenamed_WhenTheNameIsNew()
    {
        var product = New();

        product.Rename(new ProductName("Desk lamp")).ShouldBe(RenameOutcome.Renamed);

        product.Name.ShouldBe(new ProductName("Desk lamp"));
        product.Version.ShouldBe(2);
    }

    [Fact]
    public void Rename_ReturnsUnchanged_WhenTheNameIsTheSame()
    {
        var product = New();

        product.Rename(new ProductName("Lamp")).ShouldBe(RenameOutcome.Unchanged);

        product.Version.ShouldBe(1);
    }

    [Fact]
    public void Rename_ReturnsDiscontinued_WhenTheProductIsDiscontinued()
    {
        var product = New();
        product.Discontinue();

        product.Rename(new ProductName("Desk lamp")).ShouldBe(RenameOutcome.Discontinued);

        product.Name.ShouldBe(new ProductName("Lamp"));
    }

    [Fact]
    public void ReceiveStock_ReturnsReceived_WhenTheProductIsActive()
    {
        var product = New();

        product.ReceiveStock(5).ShouldBe(ReceiveStockOutcome.Received);
        product.ReceiveStock(3).ShouldBe(ReceiveStockOutcome.Received);

        product.StockOnHand.ShouldBe(8);
        product.Version.ShouldBe(3);
    }

    [Fact]
    public void ReceiveStock_ReturnsDiscontinued_WhenTheProductIsDiscontinued()
    {
        var product = New();
        product.Discontinue();

        product.ReceiveStock(5).ShouldBe(ReceiveStockOutcome.Discontinued);

        product.StockOnHand.ShouldBe(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReceiveStock_Throws_WhenTheQuantityIsNotPositive(int quantity) =>
        Should.Throw<ArgumentOutOfRangeException>(() => New().ReceiveStock(quantity));

    [Fact]
    public void Discontinue_MarksTheProductDiscontinued()
    {
        var product = New();

        product.Discontinue();

        product.IsDiscontinued.ShouldBeTrue();
        product.Version.ShouldBe(2);
    }

    [Fact]
    public void Discontinue_ChangesNothing_WhenAlreadyDiscontinued()
    {
        var product = New();
        product.Discontinue();

        product.Discontinue();

        product.Version.ShouldBe(2);
    }

    [Fact]
    public void ReserveStock_ReturnsReserved_WhenEnoughStockIsAvailable()
    {
        var product = Stocked(5);

        product.ReserveStock(Reservation(), 3).ShouldBe(ReserveStockOutcome.Reserved);

        product.Available.ShouldBe(2);
        product.StockOnHand.ShouldBe(5);
        product.Version.ShouldBe(3);
    }

    [Fact]
    public void ReserveStock_ReturnsAlreadyReserved_WhenTheReservationIsHeld()
    {
        var product = Stocked(5);
        var reservation = Reservation();
        product.ReserveStock(reservation, 3);

        product.ReserveStock(reservation, 3).ShouldBe(ReserveStockOutcome.AlreadyReserved);

        product.Available.ShouldBe(2);
        product.Version.ShouldBe(3);
    }

    [Fact]
    public void ReserveStock_ReturnsInsufficientStock_WhenOtherReservationsHoldTheStock()
    {
        var product = Stocked(5);
        product.ReserveStock(Reservation(), 4);

        product.ReserveStock(Reservation(), 2).ShouldBe(ReserveStockOutcome.InsufficientStock);

        product.Available.ShouldBe(1);
    }

    [Fact]
    public void ReserveStock_ReturnsDiscontinued_WhenTheProductIsDiscontinued()
    {
        var product = Stocked(5);
        product.Discontinue();

        product.ReserveStock(Reservation(), 1).ShouldBe(ReserveStockOutcome.Discontinued);

        product.Reservations.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReserveStock_Throws_WhenTheQuantityIsNotPositive(int quantity) =>
        Should.Throw<ArgumentOutOfRangeException>(() => Stocked(5).ReserveStock(Reservation(), quantity));

    [Fact]
    public void ReleaseStock_ReturnsReleased_AndKeepsTheReservationAsReleased()
    {
        var product = Stocked(5);
        var reservation = Reservation();
        product.ReserveStock(reservation, 3);

        product.ReleaseStock(reservation).ShouldBe(ReleaseStockOutcome.Released);

        product.Available.ShouldBe(5);
        product.Reservations.ShouldHaveSingleItem().IsReleased.ShouldBeTrue();
        product.Version.ShouldBe(4);
    }

    [Fact]
    public void ReleaseStock_ReturnsUnchanged_WhenNothingIsHeld()
    {
        var product = Stocked(5);
        var reservation = Reservation();
        product.ReserveStock(reservation, 3);
        product.ReleaseStock(reservation);

        product.ReleaseStock(reservation).ShouldBe(ReleaseStockOutcome.Unchanged);
        product.ReleaseStock(Reservation()).ShouldBe(ReleaseStockOutcome.Unchanged);

        product.Version.ShouldBe(4);
    }

    private static Product Stocked(int quantity)
    {
        var product = New();
        product.ReceiveStock(quantity);
        return product;
    }

    private static ReservationId Reservation() => new(Guid.CreateVersion7());

    private static Product New() => Product.Create(ProductId.New(), new ProductName("Lamp"), Now);
}
