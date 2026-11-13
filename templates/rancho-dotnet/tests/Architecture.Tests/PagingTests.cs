using System.Collections;
using System.Reflection;
using Platform.Application.Abstractions;
using static Architecture.Tests.Solution;

namespace Architecture.Tests;

public sealed class PagingTests
{
    private static List<(Type Query, Type Response)> Queries =>
        TypesOf(Kinds.Application, Kinds.Contracts)
            .Where(DomainTests.IsMessage)
            .Select(m => (m, Response: m.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>))
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault()))
            .Where(x => x.Response is not null)
            .Select(x => (x.m, x.Response!))
            .ToList();

    [Fact]
    public void A_query_returns_a_collection_only_as_a_page()
    {
        Queries
            .Where(q => q.Response != typeof(string) && typeof(IEnumerable).IsAssignableFrom(q.Response))
            .Select(q => $"{q.Query.Name} returns {q.Response.Name}; return Page<T> from an IPagedQuery")
            .ShouldHaveNoViolations("Every collection a query returns is paginated.");
    }

    [Fact]
    public void Page_is_the_only_page_shape()
    {
        string[] pagingMembers = [nameof(Page<int>.NextCursor), nameof(Page<int>.TotalCount)];

        TypesOf(Kinds.All.Except([Kinds.PlatformApplication]).ToArray())
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => pagingMembers.Contains(p.Name))
                .Select(p => $"{t.FullName}.{p.Name} makes a second page shape"))
            .ShouldHaveNoViolations("One page shape across the API: Page<T>. A per-feature paging envelope is a defect.");
    }

    [Fact]
    public void Every_paged_query_declares_a_maximum_within_the_ceiling()
    {
        Queries
            .Where(q => q.Response.IsGenericType && q.Response.GetGenericTypeDefinition() == typeof(Page<>))
            .SelectMany(q =>
            {
                if (!typeof(IPagedQuery).IsAssignableFrom(q.Query))
                {
                    return [$"{q.Query.Name} returns a page but is not an IPagedQuery"];
                }

                var max = (int)q.Query.GetProperty(nameof(IPagedQuery.MaxPageSize), BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
                return max is >= 1 and <= Paging.Ceiling ? [] : new[] { $"{q.Query.Name}.MaxPageSize is {max}; it must be 1 to {Paging.Ceiling}" };
            })
            .ShouldHaveNoViolations("Every paginated query declares its maximum page size, within the solution's ceiling.");
    }

    [Fact]
    public void A_paged_query_orders_only_through_its_keyset()
    {
        string[] ordering = ["OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending", "Order", "OrderDescending"];
        var pagedQueries = Queries.Where(q => typeof(IPagedQuery).IsAssignableFrom(q.Query)).Select(q => q.Query).ToHashSet();
        var handlers = TypesOf(Kinds.Infrastructure).Where(t => t.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Mediator.IQueryHandler<,>) && pagedQueries.Contains(i.GetGenericArguments()[0])))
            .ToList();

        var calls = CallsOf(handlers.SelectMany(WithNested))
            .Where(c => ordering.Any(o => c.Method.StartsWith(o + "(", StringComparison.Ordinal) || c.Method.StartsWith(o + "<", StringComparison.Ordinal)))
            .Select(c => $"{c.Origin.FullName} calls {c.Method}");

        var withoutKeyset = handlers
            .Where(h => !DependenciesOf(WithNested(h)).Any(d => d.Target.FullName.StartsWith("Platform.Application.Abstractions.Keyset`2", StringComparison.Ordinal)))
            .Select(h => $"{h.FullName} reads a page without a Keyset");

        calls.Concat(withoutKeyset)
            .ShouldHaveNoViolations("A paged query's ORDER BY and keyset predicate come from its Keyset, whose last term is a unique id.");
    }

    [Fact]
    public void Only_the_codec_seals_and_opens_cursors()
    {
        string[] protection = ["Microsoft.AspNetCore.DataProtection.IDataProtector", "Microsoft.AspNetCore.DataProtection.IDataProtectionProvider"];

        var protectors = DependenciesOf(Model.Types)
            .Where(d => protection.Contains(d.Target.FullName) && !d.Origin.FullName.StartsWith("Platform.Infrastructure.Persistence.CursorCodec", StringComparison.Ordinal))
            .Select(d => $"{d.Origin.FullName} uses {d.Target.FullName}");

        var cursors = DependenciesOf(Model.Types)
            .Where(d => d.Target.FullName == typeof(PageCursor).FullName && Owner(d.Origin)?.Kind is not (Kinds.PlatformApplication or Kinds.PlatformInfrastructure))
            .Select(d => $"{d.Origin.FullName} uses PageCursor");

        protectors.Concat(cursors)
            .ShouldHaveNoViolations("A cursor is sealed and opened only by the cursor codec, under the query's own purpose; modules never see one unsealed.");
    }
}
