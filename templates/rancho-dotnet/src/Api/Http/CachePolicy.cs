using System.Collections.Concurrent;
using Api.Endpoints;
using Microsoft.AspNetCore.Http.Metadata;
using SharedKernel;

namespace Api.Http;

/// <summary>
/// <c>Cache-Control</c> for every discovered route, decided from the class of what it returns. Every route answers one principal's authorized
/// request, so every answer is <c>private</c>; one whose success carries Confidential or Restricted data, or data whose class can't be
/// resolved, is <c>no-store</c>. Its problems are sent the same.
/// </summary>
public static class CachePolicy
{
    public const string Private = "private";

    public const string NoStore = "no-store";

    private static readonly ConcurrentDictionary<Endpoint, string> ByEndpoint = new();

    /// <summary>The header for a response body of this type.</summary>
    public static string For(Type responseType) =>
        DataClassResolver.Resolve(responseType) is { IsResolved: true, Class: < DataClass.Confidential } ? Private : NoStore;

    /// <summary>The header for a route: the strictest of its success bodies', or <c>private</c> when it returns none. Null for a route the registrar didn't map.</summary>
    public static string? For(Endpoint endpoint) =>
        endpoint.Metadata.GetMetadata<DiscoveredEndpoint>() is null
            ? null
            : ByEndpoint.GetOrAdd(endpoint, e => e.Metadata.OfType<IProducesResponseTypeMetadata>()
                .Where(m => m.StatusCode is >= 200 and < 300 && m.Type is { } type && type != typeof(void))
                .Select(m => For(m.Type!))
                .Contains(NoStore) ? NoStore : Private);

    /// <summary>Early in the pipeline, so a refusal, a rate limit or an error is sent the route's header too.</summary>
    public static IApplicationBuilder UseCachePolicy(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            if (context.GetEndpoint() is { } endpoint && For(endpoint) is { } header)
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.CacheControl = header;
                    return Task.CompletedTask;
                });
            }

            await next(context);
        });
}
