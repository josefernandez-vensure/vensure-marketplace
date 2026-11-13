using System.Net;
using System.Text.Json;
using Platform.Application.Abstractions;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

/// <summary>Keyset paging against real SQL Server, with page boundaries falling inside runs of equal sort keys.</summary>
public sealed class PagingTests(SqlServer sql)
{
    [Fact]
    public async Task Paging_skips_and_repeats_no_row_across_page_boundaries()
    {
        var client = Writer(sql);
        var prefix = $"p{Guid.NewGuid():N}"[..12];
        var created = new List<Guid>();
        foreach (var suffix in new[] { "a", "a", "a", "b", "b", "c", "c", "c", "c" })
        {
            created.Add(await CreateProduct(client, prefix + suffix));
        }

        var seen = new List<(Guid Id, string Name)>();
        string? cursor = null;
        var pages = 0;
        do
        {
            var page = await List(client, $"nameStartsWith={prefix}&pageSize=2" + (cursor is null ? "" : $"&cursor={Uri.EscapeDataString(cursor)}"));
            seen.AddRange(page.GetProperty("items").EnumerateArray().Select(i => (i.GetProperty("id").GetProperty("value").GetGuid(), i.GetProperty("name").GetString()!)));
            cursor = page.GetProperty("nextCursor").GetString();
            pages++;
        }
        while (cursor is not null && pages < 20);

        seen.Select(s => s.Id).ShouldBe(created, ignoreOrder: true);
        seen.Select(s => s.Id).Distinct().Count().ShouldBe(seen.Count);
        seen.Select(s => s.Name).ShouldBeInOrder();
        pages.ShouldBe(5);
    }

    [Fact]
    public async Task A_page_size_above_the_maximum_is_refused_never_clamped()
    {
        var client = Writer(sql);

        var tooLarge = await client.GetAsync("/catalog/products?pageSize=51", Cancel);
        var largest = await client.GetAsync("/catalog/products?pageSize=50", Cancel);

        tooLarge.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await Body(tooLarge)).GetProperty("errors").TryGetProperty("pageSize", out _).ShouldBeTrue();
        largest.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_cursor_the_query_did_not_issue_is_refused()
    {
        var client = Writer(sql);
        var prefix = $"t{Guid.NewGuid():N}"[..12];
        await CreateProduct(client, prefix + "a");
        await CreateProduct(client, prefix + "b");
        var cursor = (await List(client, $"nameStartsWith={prefix}&pageSize=1")).GetProperty("nextCursor").GetString()!;
        var tampered = cursor[..^4] + (cursor[^4] == 'A' ? 'B' : 'A') + cursor[^3..];

        var response = await client.GetAsync($"/catalog/products?nameStartsWith={prefix}&pageSize=1&cursor={Uri.EscapeDataString(tampered)}", Cancel);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemType(response)).ShouldBe(PlatformProblems.Validation.Uri);
        (await Body(response)).GetProperty("errors").TryGetProperty("cursor", out _).ShouldBeTrue();
    }

    private static async Task<JsonElement> List(HttpClient client, string query)
    {
        var response = await client.GetAsync($"/catalog/products?{query}", Cancel);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await Body(response);
    }
}
