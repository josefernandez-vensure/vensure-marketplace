using Ordering.Domain.Orders;
using Shouldly;

namespace Domain.Tests.Ordering;

public sealed class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Place_LeavesTheOrderPending_WhileItsStockIsReserved()
    {
        var id = new OrderId(Guid.CreateVersion7());

        var order = Order.Place(id, new ProductId(Guid.CreateVersion7()), 2, Now);

        order.Id.ShouldBe(id);
        order.Status.ShouldBe(OrderStatus.Pending);
        order.Quantity.ShouldBe(2);
        order.Version.ShouldBe(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Place_Throws_WhenTheQuantityIsNotPositive(int quantity) =>
        Should.Throw<ArgumentOutOfRangeException>(() => Order.Place(new OrderId(Guid.CreateVersion7()), new ProductId(Guid.CreateVersion7()), quantity, Now));

    [Fact]
    public void Confirm_ReturnsConfirmed_WhenTheOrderIsPending()
    {
        var order = New();

        order.Confirm().ShouldBe(ConfirmOutcome.Confirmed);

        order.Status.ShouldBe(OrderStatus.Confirmed);
        order.Version.ShouldBe(2);
    }

    [Fact]
    public void Confirm_ReturnsUnchanged_WhenTheOrderIsConfirmed()
    {
        var order = New();
        order.Confirm();

        order.Confirm().ShouldBe(ConfirmOutcome.Unchanged);

        order.Version.ShouldBe(2);
    }

    [Fact]
    public void Confirm_ReturnsAbandoned_WhenTheOrderWasCancelled()
    {
        var order = New();
        order.Cancel();

        order.Confirm().ShouldBe(ConfirmOutcome.Abandoned);

        order.Status.ShouldBe(OrderStatus.Cancelled);
        order.Version.ShouldBe(2);
    }

    [Fact]
    public void Reject_ReturnsRejected_WhenTheOrderIsPending()
    {
        var order = New();

        order.Reject().ShouldBe(RejectOutcome.Rejected);

        order.Status.ShouldBe(OrderStatus.Rejected);
        order.Version.ShouldBe(2);
    }

    [Fact]
    public void Reject_ReturnsUnchanged_WhenTheOrderWasCancelled()
    {
        var order = New();
        order.Cancel();

        order.Reject().ShouldBe(RejectOutcome.Unchanged);

        order.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ReturnsCancelled_WhenTheOrderIsPending()
    {
        var order = New();

        order.Cancel().ShouldBe(CancelOutcome.Cancelled);

        order.Status.ShouldBe(OrderStatus.Cancelled);
        order.Version.ShouldBe(2);
    }

    [Fact]
    public void Cancel_ReturnsUnchanged_WhenTheOrderWasRejected()
    {
        var order = New();
        order.Reject();

        order.Cancel().ShouldBe(CancelOutcome.Unchanged);

        order.Status.ShouldBe(OrderStatus.Rejected);
    }

    [Fact]
    public void Cancel_ReturnsAlreadyConfirmed_WhenTheOrderIsPastThePointOfNoReturn()
    {
        var order = New();
        order.Confirm();

        order.Cancel().ShouldBe(CancelOutcome.AlreadyConfirmed);

        order.Status.ShouldBe(OrderStatus.Confirmed);
    }

    private static Order New() => Order.Place(new OrderId(Guid.CreateVersion7()), new ProductId(Guid.CreateVersion7()), 2, Now);
}
