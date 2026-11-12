using System.Net;
using System.Net.Http.Json;
using Platform.Application.Abstractions;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

/// <summary>Every versioned write: no If-Match or <c>*</c> is 428, a stale one 412; a create-only write for an existing id is 412.</summary>
public sealed class ConcurrencyTests(SqlServer sql)
{
    [Theory]
    [InlineData(null)]
    [InlineData("*")]
    public async Task RenameProduct_ReturnsPreconditionRequired_WhenIfMatchIsMissingOrAny(string? ifMatch)
    {
        var client = Writer(sql);
        var product = await CreateProduct(client, "Original");

        var response = await Rename(client, product, "Renamed", ifMatch);

        response.StatusCode.ShouldBe((HttpStatusCode)428);
        (await ProblemType(response)).ShouldBe(PlatformProblems.PreconditionRequired.Uri);
        (await sql.Scalar<string>($"SELECT Name AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe("Original");
    }

    [Theory]
    [InlineData("\"7\"")]
    [InlineData("W/\"1\"")]
    public async Task RenameProduct_ReturnsPreconditionFailed_WhenIfMatchIsStale(string ifMatch)
    {
        var client = Writer(sql);
        var product = await CreateProduct(client, "Original");

        var response = await Rename(client, product, "Renamed", ifMatch);

        response.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
        (await ProblemType(response)).ShouldBe(PlatformProblems.PreconditionFailed.Uri);
        (await sql.Scalar<string>($"SELECT Name AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe("Original");
    }

    [Fact]
    public async Task RenameProduct_ReturnsTheNewEtag_WhenIfMatchIsCurrent()
    {
        var client = Writer(sql);
        var product = await CreateProduct(client, "Original");

        var response = await Rename(client, product, "Renamed", Tag(1));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Body(response)).GetProperty("etag").GetString().ShouldBe(Tag(2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("*")]
    public async Task ReceiveStock_ReturnsPreconditionRequired_WhenIfMatchIsMissingOrAny(string? ifMatch)
    {
        var client = Writer(sql);
        var product = await CreateProduct(client);

        var response = await ReceiveStock(client, product, 5, Guid.NewGuid().ToString(), ifMatch);

        response.StatusCode.ShouldBe((HttpStatusCode)428);
        (await sql.Scalar<int>($"SELECT StockOnHand AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe(0);
    }

    [Fact]
    public async Task ReceiveStock_ReturnsPreconditionFailed_WhenIfMatchIsStale()
    {
        var client = Writer(sql);
        var product = await CreateProduct(client);
        await ReceiveStock(client, product, 5, Guid.NewGuid().ToString(), version: 1);

        var stale = await ReceiveStock(client, product, 5, Guid.NewGuid().ToString(), version: 1);

        stale.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
        (await sql.Scalar<int>($"SELECT StockOnHand AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe(5);
    }

    [Fact]
    public async Task CreateProduct_ReturnsPreconditionFailed_WhenCreateOnlyAndTheIdExists()
    {
        var client = Writer(sql);
        var product = await CreateProduct(client, "Original");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/catalog/products") { Content = JsonContent.Create(new { id = product, name = "Again" }) };
        request.Headers.TryAddWithoutValidation("If-None-Match", "*");

        var response = await client.SendAsync(request, Cancel);

        response.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
        (await sql.Scalar<string>($"SELECT Name AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe("Original");
    }
}
