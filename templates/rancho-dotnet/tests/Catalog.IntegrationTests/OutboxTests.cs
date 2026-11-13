using System.Collections.Concurrent;
using System.Diagnostics;
using Catalog.Contracts.CommandMessages;
using Catalog.Contracts.IntegrationEvents;
using Microsoft.AspNetCore.Hosting;
using Platform.Application.Behaviors;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

/// <summary>Tests that deliver the outbox run one at a time, after the rest, so each decides when its own messages are delivered.</summary>
[CollectionDefinition(nameof(Delivery), DisableParallelization = true)]
public sealed class Delivery;

/// <summary>
/// Delivery of the outbox: under the writer's trace, to each reaction as its module's service principal on behalf of the user who started
/// the chain, keyed by the message, at least once, and by the host on its own.
/// </summary>
[Collection(nameof(Delivery))]
public sealed class OutboxTests(SqlServer sql)
{
    [Fact]
    public async Task Delivery_restores_the_writing_transactions_trace_context()
    {
        var spans = new ConcurrentBag<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == DispatchTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = spans.Add,
        };
        ActivitySource.AddActivityListener(listener);
        var order = await PlaceOrder(Buyer(sql), await StockedProduct(sql, 5), 1);
        var written = await Row(sql, "ordering", typeof(ReserveStockV1), order);
        ActivityContext.TryParse(written.TraceParent, null, out var writer).ShouldBeTrue();

        await sql.DeliverOutbox();

        var delivery = spans.Single(s => s.OperationName == DispatchTelemetry.DeliverSpan && s.GetTagItem("messaging.message.id") as string == written.Id.ToString());
        delivery.TraceId.ShouldBe(writer.TraceId, "delivery continues the trace that wrote the message");
        delivery.ParentSpanId.ShouldBe(writer.SpanId, "the delivery's span is the child of the span that wrote the message");
        sql.Api.Logs.Lines.ShouldContain(
            l => l.TraceId == writer.TraceId
                && l.Message.StartsWith("catalog.reserve-stock", StringComparison.Ordinal)
                && l.Message.Contains("succeeded", StringComparison.Ordinal),
            "the consumer's log line carries the writer's trace");
        (await sql.Scalar<string>($"SELECT TraceId AS Value FROM catalog.AuditTrail WHERE OperationId = 'catalog.reserve-stock' AND CausationId = {written.Id}"))
            .ShouldBe(writer.TraceId.ToHexString());
    }

    [Fact]
    public async Task A_reaction_runs_as_its_modules_service_principal_on_behalf_of_the_initiating_user()
    {
        var buyer = $"buyer-{Guid.NewGuid():N}";
        var order = await PlaceOrder(Buyer(sql, buyer), await StockedProduct(sql, 5), 1);

        await sql.DeliverOutbox();

        var reserve = await Row(sql, "ordering", typeof(ReserveStockV1), order);
        (await sql.Scalar<AuditRow>($"SELECT PrincipalId, OnBehalfOf, Outcome FROM catalog.AuditTrail WHERE OperationId = 'catalog.reserve-stock' AND CausationId = {reserve.Id}"))
            .ShouldBe(new AuditRow("service:catalog", buyer, "Succeeded"));
        var reply = await Row(sql, "catalog", typeof(StockReservedV1), order);
        (await sql.Scalar<AuditRow>($"SELECT PrincipalId, OnBehalfOf, Outcome FROM ordering.AuditTrail WHERE OperationId = 'ordering.confirm-order' AND CausationId = {reply.Id}"))
            .ShouldBe(new AuditRow("service:ordering", buyer, "Succeeded"));
    }

    [Fact]
    public async Task A_reaction_passes_the_initiating_user_and_its_cause_to_the_messages_it_writes()
    {
        var buyer = $"buyer-{Guid.NewGuid():N}";
        var order = await PlaceOrder(Buyer(sql, buyer), await StockedProduct(sql, 5), 1);

        await sql.DeliverOutbox();

        var reserve = await Row(sql, "ordering", typeof(ReserveStockV1), order);
        reserve.InitiatingPrincipalId.ShouldBe(buyer);
        reserve.CausationId.ShouldBeNull();
        var reply = await Row(sql, "catalog", typeof(StockReservedV1), order);
        reply.InitiatingPrincipalId.ShouldBe(buyer, "the reaction acted for the buyer, so what it writes names the buyer, not the service");
        reply.CausationId.ShouldBe(reserve.Id);
    }

    [Fact]
    public async Task A_redelivered_event_changes_nothing()
    {
        var product = await StockedProduct(sql, 5);
        var order = await PlaceOrder(Buyer(sql), product, 2);
        await sql.DeliverOutbox();
        var productVersion = await sql.Scalar<long>($"SELECT Version AS Value FROM catalog.Products WHERE Id = {product}");

        // Delivery is at least once: both messages of the exchange arrive again.
        await sql.Execute($"UPDATE ordering.OutboxMessages SET DeliveredAt = NULL WHERE Payload LIKE {"%" + order + "%"}");
        await sql.Execute($"UPDATE catalog.OutboxMessages SET DeliveredAt = NULL WHERE Payload LIKE {"%" + order + "%"}");
        await sql.DeliverOutbox();

        (await sql.Scalar<int>($"SELECT COUNT(*) AS Value FROM catalog.OutboxMessages WHERE Payload LIKE {"%" + order + "%"}")).ShouldBe(1);
        (await sql.Scalar<int>($"SELECT COUNT(*) AS Value FROM catalog.ProductReservations WHERE ProductId = {product}")).ShouldBe(1);
        (await sql.Scalar<long>($"SELECT Version AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe(productVersion);
        (await sql.Scalar<long>($"SELECT Version AS Value FROM ordering.Orders WHERE Id = {order}")).ShouldBe(2);
        (await sql.Scalar<int>($"SELECT COUNT(*) AS Value FROM ordering.OutboxMessages WHERE Payload LIKE {"%" + order + "%"} AND DeliveredAt IS NULL")).ShouldBe(0);
    }

    [Fact]
    public async Task A_message_whose_reaction_fails_stays_undelivered_for_the_next_pass()
    {
        // A reply for an order that doesn't exist: its reaction fails on every pass.
        var message = Guid.CreateVersion7();
        var payload = $$"""{"ReservationId":{"Value":"{{Guid.NewGuid()}}"},"ProductId":{"Value":"{{Guid.NewGuid()}}"},"Quantity":1}""";
        var traceParent = $"00-{ActivityTraceId.CreateRandom()}-{ActivitySpanId.CreateRandom()}-01";
        await sql.Execute($"""
            INSERT INTO catalog.OutboxMessages (Id, Type, Payload, OccurredAt, TraceParent, InitiatingPrincipalId, TenantId)
            VALUES ({message}, {typeof(StockReservedV1).FullName}, {payload}, SYSDATETIMEOFFSET(), {traceParent}, 'buyer', {CatalogApi.DefaultTenant})
            """);

        await sql.DeliverOutbox();
        await sql.DeliverOutbox();

        (await sql.Scalar<int>($"SELECT COUNT(*) AS Value FROM catalog.OutboxMessages WHERE Id = {message} AND DeliveredAt IS NULL")).ShouldBe(1);
        (await sql.Scalar<string>($"SELECT TOP 1 Outcome AS Value FROM ordering.AuditTrail WHERE OperationId = 'ordering.confirm-order' AND CausationId = {message}"))
            .ShouldBe("Failed");
        await sql.Execute($"UPDATE catalog.OutboxMessages SET DeliveredAt = SYSDATETIMEOFFSET() WHERE Id = {message}");
    }

    [Fact]
    public async Task The_outbox_is_delivered_without_being_asked()
    {
        var order = await PlaceOrder(Buyer(sql), await StockedProduct(sql, 5), 1);

        await using var host = sql.Api.WithWebHostBuilder(b => b.UseSetting("Outbox:PollInterval", "00:00:00.100"));
        _ = host.Server;

        var status = string.Empty;
        for (var waited = 0; waited < 300 && status != "Confirmed"; waited++)
        {
            await Task.Delay(100, Cancel);
            status = await sql.Scalar<string>($"SELECT Status AS Value FROM ordering.Orders WHERE Id = {order}") ?? string.Empty;
        }

        status.ShouldBe("Confirmed", "the host's own delivery runs the saga to its end");
    }

    internal static async Task<OutboxRow> Row(SqlServer sql, string schema, Type type, Guid subject)
    {
        var like = "%" + subject + "%";
        FormattableString query = schema switch
        {
            "catalog" => $"SELECT Id, TraceParent, InitiatingPrincipalId, CausationId FROM catalog.OutboxMessages WHERE Type = {type.FullName} AND Payload LIKE {like}",
            "ordering" => $"SELECT Id, TraceParent, InitiatingPrincipalId, CausationId FROM ordering.OutboxMessages WHERE Type = {type.FullName} AND Payload LIKE {like}",
            _ => throw new ArgumentOutOfRangeException(nameof(schema)),
        };
        return (await sql.Scalar<OutboxRow>(query)).ShouldNotBeNull($"{type.Name} for {subject} in {schema}'s outbox");
    }

    internal sealed record OutboxRow(Guid Id, string TraceParent, string InitiatingPrincipalId, Guid? CausationId);

    private sealed record AuditRow(string PrincipalId, string? OnBehalfOf, string Outcome);
}
