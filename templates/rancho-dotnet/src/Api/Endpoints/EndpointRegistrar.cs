using System.Reflection;
using Platform.Application.Abstractions;

namespace Api.Endpoints;

/// <summary>The only place that maps a route to a handler. Every endpoint type in this assembly is mapped, and nothing else.</summary>
public static class EndpointRegistrar
{
    public static IEndpointRouteBuilder MapDiscoveredEndpoints(this IEndpointRouteBuilder app)
    {
        var map = typeof(EndpointRegistrar).GetMethod(nameof(Map), BindingFlags.NonPublic | BindingFlags.Static)!;
        foreach (var (endpoint, message) in Discover())
        {
            map.MakeGenericMethod(endpoint, message).Invoke(null, [app]);
        }

        return app;
    }

    public static IEnumerable<(Type Endpoint, Type Message)> Discover() =>
        typeof(EndpointRegistrar).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEndpoint<>))
                .Select(i => (t, i.GetGenericArguments()[0])));

    private static void Map<TEndpoint, TMessage>(IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint<TMessage>
        where TMessage : IDeclaresOperation
    {
        var operation = TMessage.Operation;
        app.MapMethods(TEndpoint.Pattern, [TEndpoint.Method], TEndpoint.Handler)
            .WithName(operation.Id)
            .RequireAuthorization(operation.Policy)
            .RequireRateLimiting(operation.RateLimit)
            .WithMetadata(new DiscoveredEndpoint(typeof(TEndpoint), operation));
    }
}
