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

    private static Product New() => Product.Create(ProductId.New(), new ProductName("Lamp"), Now);
}
