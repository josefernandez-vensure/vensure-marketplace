using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace Api.Http;

/// <summary>
/// The CORS policy for clients served from another origin: only the origins configured under <c>Cors:Origins</c>, none by default. It admits
/// every request header the contract uses and exposes every response header a client reads. A header added to the contract is added here.
/// </summary>
public static class CrossOrigin
{
    public const string Policy = "contract";

    /// <summary>The request headers of the contract: credentials, bodies, preconditions, idempotency keys and trace context.</summary>
    public static readonly IReadOnlyList<string> RequestHeaders =
        ["Authorization", "Content-Type", "If-Match", "If-None-Match", "Idempotency-Key", "traceparent", "tracestate"];

    /// <summary>The response headers a client reads: a resource's version, when to retry, and where a create put its resource.</summary>
    public static readonly IReadOnlyList<string> ResponseHeaders = ["ETag", "Retry-After", "Location"];

    private static readonly string[] Methods = [HttpMethods.Get, HttpMethods.Post, HttpMethods.Put, HttpMethods.Patch, HttpMethods.Delete];

    public static IServiceCollection AddCrossOrigin(this IServiceCollection services)
    {
        services.AddOptions<CrossOriginOptions>()
            .BindConfiguration(CrossOriginOptions.Section)
            .Validate(o => o.Origins.All(IsOrigin), "Each of Cors:Origins is a scheme, host and optional port, such as https://app.example.com: no path, no trailing slash, no wildcard.")
            .ValidateOnStart();
        services.AddCors();
        services.AddSingleton<IConfigureOptions<CorsOptions>, ConfigureCors>();
        return services;
    }

    private static bool IsOrigin(string origin) =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
        && uri.PathAndQuery == "/"
        && !origin.EndsWith('/')
        && string.IsNullOrEmpty(uri.UserInfo);

    private sealed class ConfigureCors(IOptions<CrossOriginOptions> origins) : IConfigureOptions<CorsOptions>
    {
        public void Configure(CorsOptions options) =>
            options.AddPolicy(Policy, policy => policy
                .WithOrigins([.. origins.Value.Origins])
                .WithMethods(Methods)
                .WithHeaders([.. RequestHeaders])
                .WithExposedHeaders([.. ResponseHeaders]));
    }
}

public sealed class CrossOriginOptions
{
    public const string Section = "Cors";

    /// <summary>The origins a client may be served from. Empty admits no other origin.</summary>
    public IReadOnlyList<string> Origins { get; set; } = [];
}
