using System.Net;
using Catalog.Application;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

/// <summary>A command over several aggregates of one module, through a domain service: all of them commit together, or none does.</summary>
public sealed class RenameProductsTests(SqlServer sql)
{
    [Fact]
    public async Task RenameProducts_RenamesEveryProduct_InOneCommitWithOneAuditRecord()
    {
        var client = Writer(sql);
        var lamp = await CreateProduct(client, "Lamp");
        var desk = await CreateProduct(client, "Desk");

        var response = await RenameProducts(client, (lamp, "Floor lamp"), (desk, "Writing desk"));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await sql.Scalar<string>($"SELECT Name AS Value FROM catalog.Products WHERE Id = {lamp}")).ShouldBe("Floor lamp");
        (await sql.Scalar<string>($"SELECT Name AS Value FROM catalog.Products WHERE Id = {desk}")).ShouldBe("Writing desk");
        (await sql.Scalar<string>($"SELECT RecordIds AS Value FROM catalog.AuditTrail WHERE OperationId = 'catalog.rename-products' AND RecordIds LIKE {"%" + lamp + "%"}"))
            .ShouldBe($"{lamp},{desk}");
    }

    [Fact]
    public async Task RenameProducts_RenamesNone_WhenOneIsDiscontinued()
    {
        var client = Writer(sql);
        var lamp = await CreateProduct(client, "Lamp");
        var desk = await CreateProduct(client, "Desk");
        await sql.Execute($"UPDATE catalog.Products SET IsDiscontinued = 1 WHERE Id = {desk}");

        var response = await RenameProducts(client, (lamp, "Floor lamp"), (desk, "Writing desk"));

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ProblemType(response)).ShouldBe(CatalogProblems.ProductDiscontinued.Uri);
        (await sql.Scalar<string>($"SELECT Name AS Value FROM catalog.Products WHERE Id = {lamp}")).ShouldBe("Lamp");
    }

    [Fact]
    public async Task RenameProducts_RenamesNone_WhenOneDoesNotExist()
    {
        var client = Writer(sql);
        var lamp = await CreateProduct(client, "Lamp");

        var response = await RenameProducts(client, (lamp, "Floor lamp"), (Guid.CreateVersion7(), "Ghost"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await sql.Scalar<string>($"SELECT Name AS Value FROM catalog.Products WHERE Id = {lamp}")).ShouldBe("Lamp");
    }

    [Fact]
    public async Task RenameProducts_IsRefused_WhenItNamesMoreProductsThanItsLimit()
    {
        var products = Enumerable.Range(0, Application.RenameProducts.RenameProducts.MaxProducts + 1).Select(i => (Guid.CreateVersion7(), $"Product {i}")).ToArray();

        var response = await RenameProducts(Writer(sql), products);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
