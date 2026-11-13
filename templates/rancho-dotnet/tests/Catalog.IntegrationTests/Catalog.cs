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

    /// <summary>Receives stock as a client that last read <paramref name="version"/>; a new product is at version 1.</summary>
    public static async Task<HttpResponseMessage> ReceiveStock(HttpClient client, Guid product, int quantity, string? key, long version = 1) =>
        await ReceiveStock(client, product, quantity, key, Tag(version));

    /// <summary>Receives stock sending <paramref name="ifMatch"/> verbatim, or no If-Match at all.</summary>
    public static async Task<HttpResponseMessage> ReceiveStock(HttpClient client, Guid product, int quantity, string? key, string? ifMatch)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/catalog/products/{product}/stock-receipts")
        {
            Content = JsonContent.Create(new { quantity }),
        };
        if (ifMatch is not null)
        {
            request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        }

        if (key is not null)
        {
            request.Headers.Add("Idempotency-Key", key);
        }

        return await client.SendAsync(request, Cancel);
    }

    /// <summary>Renames sending <paramref name="ifMatch"/> verbatim, or no If-Match at all.</summary>
    public static async Task<HttpResponseMessage> Rename(HttpClient client, Guid product, string name, string? ifMatch)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/catalog/products/{product}/rename")
        {
            Content = JsonContent.Create(new { name }),
        };
        if (ifMatch is not null)
        {
            request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        }

        return await client.SendAsync(request, Cancel);
    }

    public static HttpClient Buyer(SqlServer sql, string subject = "buyer") => sql.Api.ClientFor(subject, "ordering.read", "ordering.write");

    public static async Task<Guid> PlaceOrder(HttpClient client, Guid product, int quantity)
    {
        var id = Guid.CreateVersion7();
        var response = await client.PostAsJsonAsync("/ordering/orders", new { id, productId = product, quantity }, Cancel);
        response.EnsureSuccessStatusCode();
        return id;
    }

    /// <summary>Cancels sending <paramref name="ifMatch"/> verbatim, or no If-Match at all.</summary>
    public static async Task<HttpResponseMessage> CancelOrder(HttpClient client, Guid order, string? ifMatch)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/ordering/orders/{order}/cancel");
        if (ifMatch is not null)
        {
            request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        }

        return await client.SendAsync(request, Cancel);
    }

    /// <summary>A product with <paramref name="stock"/> on hand, at version 2.</summary>
    public static async Task<Guid> StockedProduct(SqlServer sql, int stock)
    {
        var client = Writer(sql);
        var product = await CreateProduct(client);
        (await ReceiveStock(client, product, stock, Guid.NewGuid().ToString())).EnsureSuccessStatusCode();
        return product;
    }

    /// <summary>The strong entity tag of a version, as the API sends it.</summary>
    public static string Tag(long version) => $"\"{version}\"";

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
