using System.Net;
using Platform.Application.Abstractions;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

/// <summary>ReceiveStock declares an idempotency key: receiving twice would double the stock.</summary>
public sealed class IdempotencyTests(SqlServer sql)
{
    [Fact]
    public async Task A_keyed_command_without_a_key_is_refused()
    {
        var client = Writer(sql);
        var product = await CreateProduct(client);

        var response = await ReceiveStock(client, product, 5, key: null);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemType(response)).ShouldBe(PlatformProblems.IdempotencyKeyRequired.Uri);
        (await sql.Scalar<int>($"SELECT StockOnHand AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe(0);
    }

    [Fact]
    public async Task A_replay_returns_the_recorded_result_without_running_the_handler_again()
    {
        var client = Writer(sql);
        var product = await CreateProduct(client);
        var key = Guid.NewGuid().ToString();

        var first = await ReceiveStock(client, product, 5, key);
        var second = await ReceiveStock(client, product, 5, key);

        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        second.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Body(second)).GetRawText().ShouldBe((await Body(first)).GetRawText());
        (await sql.Scalar<int>($"SELECT StockOnHand AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe(5);
    }

    [Fact]
    public async Task The_same_key_with_a_different_request_is_refused()
    {
        var client = Writer(sql);
        var product = await CreateProduct(client);
        var key = Guid.NewGuid().ToString();

        await ReceiveStock(client, product, 5, key);
        var reused = await ReceiveStock(client, product, 7, key);

        reused.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await ProblemType(reused)).ShouldBe(PlatformProblems.IdempotencyKeyReused.Uri);
        (await sql.Scalar<int>($"SELECT StockOnHand AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe(5);
    }

    [Fact]
    public async Task Concurrent_duplicates_resolve_as_one_execution_and_replays()
    {
        var client = Writer(sql);
        var product = await CreateProduct(client);
        var key = Guid.NewGuid().ToString();

        var responses = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => ReceiveStock(client, product, 5, key)));

        responses.ShouldAllBe(r => r.StatusCode == HttpStatusCode.OK);
        var bodies = await Task.WhenAll(responses.Select(Body));
        bodies.Select(b => b.GetRawText()).Distinct().Count().ShouldBe(1);
        (await sql.Scalar<int>($"SELECT StockOnHand AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe(5);
    }

    [Fact]
    public async Task A_key_belongs_to_its_principal()
    {
        var product = await CreateProduct(Writer(sql));
        var key = Guid.NewGuid().ToString();

        await ReceiveStock(Writer(sql, "alice"), product, 5, key);
        var bob = await ReceiveStock(Writer(sql, "bob"), product, 5, key);

        bob.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await sql.Scalar<int>($"SELECT StockOnHand AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe(10);
    }
}
