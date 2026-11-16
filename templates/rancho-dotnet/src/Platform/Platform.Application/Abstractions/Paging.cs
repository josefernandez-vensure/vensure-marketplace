using System.Text.Json;
using System.Text.Json.Nodes;
using SharedKernel;

namespace Platform.Application.Abstractions;

/// <summary>The one page shape across the API. <see cref="NextCursor"/> is null on the last page.</summary>
public sealed record Page<T>(IReadOnlyList<T> Items, [Internal] string? NextCursor, [Internal] long? TotalCount = null);

/// <summary>
/// A query that returns a page. The paging behavior refuses a page size outside 1..<see cref="MaxPageSize"/>
/// and a cursor that isn't this query's, before the handler runs: a request is rejected, never clamped.
/// </summary>
public interface IPagedQuery
{
    static abstract int MaxPageSize { get; }

    int PageSize { get; }

    string? Cursor { get; }
}

public interface IPagedQuery<TItem> : IQuery<Page<TItem>>, IPagedQuery;

public static class Paging
{
    /// <summary>No query may declare a larger maximum page size.</summary>
    public const int Ceiling = 100;

    public const int DefaultPageSize = 20;

    /// <summary>
    /// What a cursor is bound to: the query's operation and every parameter but the page size and the cursor, its sort and its filters
    /// included. A cursor replayed under another sort or filter isn't this query's, so it is refused, never read as another ordering's key.
    /// </summary>
    public static string CursorPurpose<TQuery>(TQuery query)
        where TQuery : IPagedQuery, IDeclaresOperation
    {
        var parameters = (JsonObject)JsonSerializer.SerializeToNode(query, query.GetType())!;
        parameters.Remove(nameof(IPagedQuery.PageSize));
        parameters.Remove(nameof(IPagedQuery.Cursor));
        return $"{TQuery.Operation.Id}:{parameters.ToJsonString()}";
    }
}

/// <summary>Where a page ended: exactly the sort key and the unique id. Clients only ever see it encrypted.</summary>
public sealed record PageCursor(string SortKey, Guid Id);

/// <summary>
/// Seals and opens cursors with Data Protection, under the purpose <see cref="Paging.CursorPurpose{TQuery}"/> names, so a cursor works only for
/// the query, sort and filters that issued it.
/// </summary>
public interface ICursorCodec
{
    string Seal(PageCursor cursor, string purpose);

    /// <summary>Null when the token was tampered with, expired, or issued for another purpose.</summary>
    PageCursor? Open(string token, string purpose);
}
