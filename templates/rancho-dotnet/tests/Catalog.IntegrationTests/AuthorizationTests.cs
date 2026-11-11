using System.Net;
using System.Net.Http.Json;
using Catalog.Application.RenameProduct;
using Catalog.Contracts;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

/// <summary>Every failed authorization is refused and recorded, whichever layer refused it.</summary>
public sealed class AuthorizationTests(SqlServer sql)
{
    [Fact]
    public async Task A_principal_without_the_policy_is_refused_at_the_transport_and_recorded()
    {
        var subject = $"reader-{Guid.NewGuid():N}";
        var client = sql.Api.ClientFor(subject, "catalog.read");

        var response = await client.PostAsJsonAsync($"/catalog/products/{Guid.NewGuid()}/rename", new { name = "Lamp" }, Cancel);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ProblemType(response)).ShouldBe(PlatformProblems.Forbidden.Uri);
        (await sql.Scalar<string>($"SELECT Outcome AS Value FROM catalog.AuditTrail WHERE PrincipalId = {subject}"))
            .ShouldBe(nameof(AuditOutcome.Denied));
    }

    [Fact]
    public async Task The_authorization_behavior_refuses_and_records_without_any_transport_filter()
    {
        // Dispatched directly, as a job or an agent would: no endpoint, no [Authorize], only the pipeline.
        var subject = $"job-{Guid.NewGuid():N}";
        var product = Guid.NewGuid();
        using var scope = sql.Api.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext { User = CatalogApi.Principal(subject, "catalog.read") };

        var result = await scope.ServiceProvider.GetRequiredService<ISender>()
            .Send(new RenameProduct(new ProductId(product), "Lamp"), Cancel);

        result.Problem?.Type.ShouldBe(PlatformProblems.Forbidden);
        (await sql.Scalar<string>($"SELECT RecordIds AS Value FROM catalog.AuditTrail WHERE PrincipalId = {subject} AND Outcome = 'Denied'"))
            .ShouldBe(product.ToString());
    }
}
