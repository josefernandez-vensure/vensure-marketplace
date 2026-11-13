using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Platform.Application.Abstractions;
using Shouldly;

namespace Api.Tests;

/// <summary>
/// The pipeline's answers that need no database: each short-circuits before the handler or the audit trail.
/// What needs SQL Server is in Catalog.IntegrationTests.
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
