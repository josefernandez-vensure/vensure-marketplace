using System.Net;
using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

public sealed class QueryAndMigrationTests(SqlServer sql)
{
    [Fact]
    public async Task A_query_that_reads_with_sql_projects_what_was_committed()
    {
        var client = Writer(sql);
        var product = await CreateProduct(client, "Desk lamp");
        await ReceiveStock(client, product, 3, Guid.NewGuid().ToString());

        var response = await client.GetAsync($"/catalog/products/{product}", Cancel);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await Body(response);
        body.GetProperty("name").GetString().ShouldBe("Desk lamp");
        body.GetProperty("stockOnHand").GetInt32().ShouldBe(3);
        body.GetProperty("eTag").GetString().ShouldBe("\"2\"");
    }

    [Fact]
    public async Task A_query_for_a_missing_record_answers_its_declared_problem()
    {
        var response = await Writer(sql).GetAsync($"/catalog/products/{Guid.NewGuid()}", Cancel);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ProblemType(response)).ShouldBe(Application.CatalogProblems.ProductNotFound.Uri);
    }

    [Fact]
    public async Task Migrations_run_clean_twice_and_leave_nothing_pending()
    {
        // The fixture already applied them twice to an empty database; once more must change nothing.
        await sql.Migrate();

        using var scope = sql.Api.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database;
        (await database.GetPendingMigrationsAsync(Cancel)).ShouldBeEmpty();
        (await database.GetAppliedMigrationsAsync(Cancel)).ShouldBe(database.GetMigrations());
    }
}
