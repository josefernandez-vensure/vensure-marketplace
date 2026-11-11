using System.Net;
using System.Net.Http.Json;
using Catalog.Application;
using Catalog.Application.RenameProduct;
using Catalog.Contracts;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

/// <summary>What one commit holds: the change, its outbox rows, and its audit record; and what a failure leaves behind. The rollback itself is proven in Platform.Tests.</summary>
public sealed class UnitOfWorkTests(SqlServer sql)
{
    [Fact]
    public async Task A_success_commits_the_change_its_outbox_rows_and_its_audit_record_together()
    {
        var product = await CreateProduct(Writer(sql, "creator"));

        (await sql.Scalar<int>($"SELECT COUNT(*) AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe(1);
        (await sql.Scalar<string>($"SELECT Outcome AS Value FROM catalog.AuditTrail WHERE OperationId = 'catalog.create-product' AND RecordIds = {product.ToString()}"))
            .ShouldBe(nameof(AuditOutcome.Succeeded));
        var traceParent = await sql.Scalar<string>($"SELECT TraceParent AS Value FROM catalog.OutboxMessages WHERE Payload LIKE {"%" + product + "%"}");
        traceParent.ShouldNotBeNull().ShouldMatch("^00-[0-9a-f]{32}-[0-9a-f]{16}-0[01]$");
        (await sql.Scalar<string>($"SELECT InitiatingPrincipalId AS Value FROM catalog.OutboxMessages WHERE Payload LIKE {"%" + product + "%"}"))
            .ShouldBe("creator");
    }

    [Fact]
    public async Task A_failure_is_recorded_and_commits_nothing_else()
    {
        var client = Writer(sql);
        var product = await CreateProduct(client, "Original");
        await sql.Execute($"UPDATE catalog.Products SET IsDiscontinued = 1 WHERE Id = {product}");

        var response = await client.PostAsJsonAsync($"/catalog/products/{product}/rename", new { name = "Renamed" }, Cancel);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ProblemType(response)).ShouldBe(CatalogProblems.ProductDiscontinued.Uri);
        (await sql.Scalar<string>($"SELECT Name AS Value FROM catalog.Products WHERE Id = {product}")).ShouldBe("Original");
        (await sql.Scalar<string>($"SELECT Outcome AS Value FROM catalog.AuditTrail WHERE OperationId = 'catalog.rename-product' AND RecordIds = {product.ToString()}"))
            .ShouldBe(nameof(AuditOutcome.Failed));
    }

    [Fact]
    public async Task A_stale_write_is_a_conflict_not_an_exception()
    {
        var product = await CreateProduct(Writer(sql));
        using var scope = sql.Api.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext { User = CatalogApi.Principal("writer", "catalog.write") };
        var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.CatalogDbContext>();

        // Load the product into this scope, then let someone else change it first.
        _ = await db.Products.SingleAsync(p => p.Id == new Domain.Products.ProductId(product), Cancel);
        await sql.Execute($"UPDATE catalog.Products SET Version = Version + 1 WHERE Id = {product}");

        var result = await scope.ServiceProvider.GetRequiredService<ISender>()
            .Send(new RenameProduct(new ProductId(product), "Too late"), Cancel);

        result.Problem?.Type.ShouldBe(PlatformProblems.Conflict);
    }
}
