using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;

namespace Catalog.IntegrationTests;

/// <summary>Small helpers over the Catalog API, so each test reads as what it proves.</summary>
internal static class Catalog
{
    public static CancellationToken Cancel => TestContext.Current.CancellationToken;

    public static HttpClient Writer(SqlServer sql, string subject = "writer") => sql.Api.ClientFor(subject, "catalog.read", "catalog.write");

    public static async Task<Guid> CreateProduct(HttpClient client, string name = "Lamp")
    {
        var id = Guid.CreateVersion7();
        var response = await client.PostAsJsonAsync("/catalog/products", new { id, name }, Cancel);
        response.EnsureSuccessStatusCode();
        return id;
    }

    public static async Task<HttpResponseMessage> ReceiveStock(HttpClient client, Guid product, int quantity, string? key)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/catalog/products/{product}/stock-receipts")
        {
            Content = JsonContent.Create(new { quantity }),
        };
        if (key is not null)
        {
            request.Headers.Add("Idempotency-Key", key);
        }

        return await client.SendAsync(request, Cancel);
    }

    public static async Task<JsonElement> Body(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancel));
        return document.RootElement.Clone();
    }

    public static async Task<string?> ProblemType(HttpResponseMessage response)
    {
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        return (await Body(response)).GetProperty("type").GetString();
    }
}
