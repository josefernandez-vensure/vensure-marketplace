using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Catalog.Application.RenameProduct;
using Catalog.Contracts;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Shouldly;

namespace Api.Tests;

/// <summary>
/// The pipeline's answers that need no database: each short-circuits before the handler.
/// They prove the behaviors run, in order, for every caller.
/// </summary>
public sealed class PipelineTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly object Rename = new { name = "Lamp" };

    [Fact]
    public async Task An_anonymous_caller_is_refused_with_problem_details()
    {
        var response = await factory.CreateClient().PostAsJsonAsync($"/catalog/products/{Guid.NewGuid()}/rename", Rename, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await ProblemType(response)).ShouldBe(PlatformProblems.Unauthenticated.Uri);
    }

    [Fact]
    public async Task A_principal_without_the_policy_is_refused()
    {
        var client = factory.ClientFor("reader", "catalog.read");

        var response = await client.PostAsJsonAsync($"/catalog/products/{Guid.NewGuid()}/rename", Rename, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ProblemType(response)).ShouldBe(PlatformProblems.Forbidden.Uri);
    }

    [Fact]
    public async Task The_authorization_behavior_refuses_without_any_transport_filter()
    {
        // Dispatched directly, as a job or an agent would: no endpoint, no [Authorize], only the pipeline.
        using var scope = factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext { User = ApiFactory.Principal("reader", "catalog.read") };

        var result = await scope.ServiceProvider.GetRequiredService<ISender>()
            .Send(new RenameProduct(new ProductId(Guid.NewGuid()), "Lamp"), TestContext.Current.CancellationToken);

        result.Problem?.Type.ShouldBe(PlatformProblems.Forbidden);
    }

    [Fact]
    public async Task An_invalid_message_is_refused_before_its_handler_with_field_errors_by_request_path()
    {
        var client = factory.ClientFor("writer", "catalog.write");

        var response = await client.PostAsJsonAsync($"/catalog/products/{Guid.NewGuid()}/rename", new { name = "" }, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        body.RootElement.GetProperty("type").GetString().ShouldBe(PlatformProblems.Validation.Uri);
        body.RootElement.GetProperty("errors").TryGetProperty("name", out _).ShouldBeTrue();
        body.RootElement.GetProperty("traceId").GetString()!.Length.ShouldBe(32);
    }

    private static async Task<string?> ProblemType(HttpResponseMessage response)
    {
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return body.RootElement.GetProperty("type").GetString();
    }
}
