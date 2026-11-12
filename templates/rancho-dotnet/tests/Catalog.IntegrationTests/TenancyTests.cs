using System.Net;
using System.Net.Http.Json;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

/// <summary>A principal that holds the policy but not the record: another tenant's product is never read, listed or changed.</summary>
public sealed class TenancyTests(SqlServer sql)
{
    private static readonly Guid OtherTenant = Guid.Parse("7a1c2d3e-0000-4000-8000-000000000002");

    private HttpClient Outsider => sql.Api.ClientFor("outsider", OtherTenant, "catalog.read", "catalog.write");

    [Fact]
    public async Task GetProduct_ReturnsNotFound_WhenTheProductIsAnotherTenants()
    {
        var product = await CreateProduct(Writer(sql));

        var response = await Outsider.GetAsync($"/catalog/products/{product}", Cancel);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListProducts_ReturnsNoProduct_OfAnotherTenant()
    {
        var prefix = $"x{Guid.NewGuid():N}"[..12];
        await CreateProduct(Writer(sql), prefix + "mine");

        var response = await Outsider.GetAsync($"/catalog/products?nameStartsWith={prefix}", Cancel);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Body(response)).GetProperty("items").GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public async Task RenameProduct_ReturnsNotFound_WhenTheProductIsAnotherTenants()
    {
        var product = await CreateProduct(Writer(sql), "Original");

        var response = await Rename(Outsider, product, "Taken", Tag(1));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await sql.Scalar<string>($"SELECT Name AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe("Original");
    }

    [Fact]
    public async Task ReceiveStock_ReturnsNotFound_WhenTheProductIsAnotherTenants()
    {
        var product = await CreateProduct(Writer(sql));

        var response = await ReceiveStock(Outsider, product, 5, Guid.NewGuid().ToString());

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await sql.Scalar<int>($"SELECT StockOnHand AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe(0);
    }

    [Fact]
    public async Task CreateProduct_ChangesNothing_WhenTheIdIsAnotherTenants()
    {
        var product = await CreateProduct(Writer(sql), "Original");

        var response = await Outsider.PostAsJsonAsync("/catalog/products", new { id = product, name = "Taken" }, Cancel);

        response.IsSuccessStatusCode.ShouldBeFalse();
        (await sql.Scalar<string>($"SELECT Name AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe("Original");
        (await sql.Scalar<Guid>($"SELECT TenantId AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe(CatalogApi.DefaultTenant);
    }
}
