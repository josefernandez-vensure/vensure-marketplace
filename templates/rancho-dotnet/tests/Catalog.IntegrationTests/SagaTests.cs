using System.Net;
using Catalog.Contracts.CommandMessages;
using Ordering.Application;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

/// <summary>
/// The order saga, choreographed between Ordering and Catalog: an order asks Catalog to reserve its stock (the compensable step), and
/// Catalog's reply confirms it (the point of no return) or rejects it. An order abandoned in between releases its stock.
/// </summary>
[Collection(nameof(Delivery))]
public sealed class SagaTests(SqlServer sql)
{
    [Fact]
    public async Task An_order_whose_stock_is_reserved_is_confirmed()
    {
        var buyer = Buyer(sql);
        var product = await StockedProduct(sql, 5);
        var order = await PlaceOrder(buyer, product, 2);
        (await Status(order)).ShouldBe("Pending");

        await sql.DeliverOutbox();

        var response = await buyer.GetAsync($"/ordering/orders/{order}", Cancel);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Body(response)).GetProperty("status").GetString().ShouldBe("Confirmed");
        (await Held(product)).ShouldBe(2);
    }

    [Fact]
    public async Task An_order_whose_stock_cannot_be_reserved_is_rejected()
    {
        var product = await StockedProduct(sql, 1);
        var order = await PlaceOrder(Buyer(sql), product, 2);

        await sql.DeliverOutbox();

        (await Status(order)).ShouldBe("Rejected");
        (await Held(product)).ShouldBe(0);
    }

    [Fact]
    public async Task A_saga_step_that_fails_after_a_compensable_one_runs_its_compensations()
    {
        var buyer = Buyer(sql);
        var product = await StockedProduct(sql, 5);
        var order = await PlaceOrder(buyer, product, 2);

        // Cancelled while its stock is being reserved: the reservation succeeds, then confirmation finds the order abandoned.
        (await CancelOrder(buyer, order, Tag(1))).StatusCode.ShouldBe(HttpStatusCode.OK);
        await sql.DeliverOutbox();

        (await Status(order)).ShouldBe("Cancelled");
        (await sql.Scalar<int>($"SELECT COUNT(*) AS Value FROM ordering.OutboxMessages WHERE Type = {typeof(ReleaseStockV1).FullName} AND Payload LIKE {"%" + order + "%"} AND DeliveredAt IS NOT NULL"))
            .ShouldBe(1, "the compensation was sent and delivered");
        (await sql.Scalar<bool>($"SELECT IsReleased AS Value FROM catalog.ProductReservations WHERE ProductId = {product} AND Id = {order}"))
            .ShouldBeTrue("the compensation records the release; it deletes nothing");
        (await Held(product)).ShouldBe(0);
    }

    [Fact]
    public async Task CancelOrder_ReturnsOrderAlreadyConfirmed_WhenTheOrderIsConfirmed()
    {
        var buyer = Buyer(sql);
        var order = await PlaceOrder(buyer, await StockedProduct(sql, 5), 1);
        await sql.DeliverOutbox();

        var response = await CancelOrder(buyer, order, Tag(2));

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ProblemType(response)).ShouldBe(OrderingProblems.OrderAlreadyConfirmed.Uri);
        (await Status(order)).ShouldBe("Confirmed");
    }

    private Task<string?> Status(Guid order) => sql.Scalar<string>($"SELECT Status AS Value FROM ordering.Orders WHERE Id = {order}");

    private async Task<int> Held(Guid product) =>
        await sql.Scalar<int?>($"SELECT SUM(Quantity) AS Value FROM catalog.ProductReservations WHERE ProductId = {product} AND IsReleased = 0") ?? 0;
}
