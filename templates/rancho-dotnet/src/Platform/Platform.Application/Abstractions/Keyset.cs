using System.Linq.Expressions;
using System.Text.Json;

namespace Platform.Application.Abstractions;

/// <summary>
/// A paged query's ordering, declared once: one sort key, then a unique id that breaks ties.
/// It is the only source of the ORDER BY and of the keyset predicate, so a page boundary never skips or repeats a row.
/// </summary>
public sealed class Keyset<TRow, TKey>
{
    private readonly Expression<Func<TRow, TKey>> _sortKey;
    private readonly Expression<Func<TRow, Guid>> _uniqueId;
    private readonly Func<TRow, TKey> _sortKeyOf;
    private readonly Func<TRow, Guid> _uniqueIdOf;

    public Keyset(Expression<Func<TRow, TKey>> sortKey, Expression<Func<TRow, Guid>> uniqueId)
    {
        _sortKey = sortKey;
        _uniqueId = uniqueId;
        _sortKeyOf = sortKey.Compile();
        _uniqueIdOf = uniqueId.Compile();
    }

    /// <summary>The query's rows after its cursor, in order, one more than a page so the handler knows whether another follows.</summary>
    public IQueryable<TRow> Read<TQuery>(IQueryable<TRow> rows, TQuery query, ICursorCodec cursors)
        where TQuery : IPagedQuery, IDeclaresOperation
    {
        if (query.Cursor is { } token && cursors.Open(token, TQuery.Operation.Id) is { } after)
        {
            rows = rows.Where(After(after));
        }

        return rows.OrderBy(_sortKey).ThenBy(_uniqueId).Take(query.PageSize + 1);
    }

    public Page<TItem> Page<TQuery, TItem>(IReadOnlyList<TRow> rows, TQuery query, Func<TRow, TItem> map, ICursorCodec cursors)
        where TQuery : IPagedQuery, IDeclaresOperation
    {
        var items = rows.Take(query.PageSize).ToList();
        var next = rows.Count > query.PageSize
            ? cursors.Seal(new PageCursor(JsonSerializer.Serialize(_sortKeyOf(items[^1])), _uniqueIdOf(items[^1])), TQuery.Operation.Id)
            : null;
        return new Page<TItem>(items.Select(map).ToList(), next);
    }

    /// <summary>sortKey &gt; cursor.SortKey, or the same sort key and uniqueId &gt; cursor.Id.</summary>
    private Expression<Func<TRow, bool>> After(PageCursor cursor)
    {
        var row = Expression.Parameter(typeof(TRow), "row");
        var key = Rebind(_sortKey, row);
        var id = Rebind(_uniqueId, row);

        // Values travel as captured members, so the database sees parameters, never literals.
        var keyValue = Expression.Property(Expression.Constant(new Boxed<TKey>(JsonSerializer.Deserialize<TKey>(cursor.SortKey)!)), nameof(Boxed<TKey>.Value));
        var idValue = Expression.Property(Expression.Constant(new Boxed<Guid>(cursor.Id)), nameof(Boxed<Guid>.Value));

        var later = Expression.GreaterThan(Compare(key, keyValue), Expression.Constant(0));
        var tie = Expression.AndAlso(Expression.Equal(key, keyValue), Expression.GreaterThan(Compare(id, idValue), Expression.Constant(0)));
        return Expression.Lambda<Func<TRow, bool>>(Expression.OrElse(later, tie), row);
    }

    private static MethodCallExpression Compare(Expression left, Expression right) =>
        Expression.Call(left, left.Type.GetMethod(nameof(IComparable<int>.CompareTo), [left.Type])!, right);

    private static Expression Rebind<T>(Expression<Func<TRow, T>> selector, ParameterExpression row) =>
        new Replace(selector.Parameters[0], row).Visit(selector.Body);

    private sealed record Boxed<T>(T Value);

    private sealed class Replace(ParameterExpression from, Expression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;
    }
}
