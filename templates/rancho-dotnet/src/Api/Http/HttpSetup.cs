using System.Diagnostics;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions;

namespace Api.Http;

/// <summary>The composition root's HTTP concerns: the dispatch context, problem details, deny-by-default, and rate limits.</summary>
public static class HttpSetup
{
    public static IServiceCollection AddHttpPlatform(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped(sp =>
        {
            var http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext
                ?? throw new InvalidOperationException("An HTTP dispatch needs a request; jobs and reactions build their own DispatchContext.");
            // The composition root decides who the principal is, the token's subject, and which records it may see,
            // the token's tenant. Application code never reads a claim.
            return new DispatchContext(
                http.User,
                http.User.FindFirst("sub")?.Value ?? "anonymous",
                CurrentTraceId(),
                Guid.TryParse(http.User.FindFirst("tenant")?.Value, out var tenant) ? new TenantId(tenant) : null,
                http.Request.Headers["Idempotency-Key"].FirstOrDefault());
        });

        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var details = context.ProblemDetails;
            if (details.Type is null || details.Type.StartsWith("https://tools.ietf.org/", StringComparison.Ordinal))
            {
                // Framework answers (401, 403, 404, 429, 500) get the platform's declared type, never a per-status default.
                var declared = PlatformProblems.All.FirstOrDefault(p => p.Status == details.Status) ?? PlatformProblems.Unexpected;
                details.Type = declared.Uri;
                details.Title = declared.Title;
            }

            details.Extensions["traceId"] = CurrentTraceId();
        });

        // Deny by default: a route with no policy of its own is refused, authenticated or not.
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationMiddlewareResultHandler, AuditingAuthorizationResultHandler>();
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAssertion(_ => false).Build());

        services.AddOptions<RateLimitOptions>()
            .BindConfiguration(RateLimitOptions.Section)
            .Validate(o => o.PermitLimit > 0 && o.Window > TimeSpan.Zero, "Rate limits need a positive permit limit and window; a missing limit denies.")
            .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            options.AddPolicy(RateLimits.Default, context =>
            {
                var limits = context.RequestServices.GetRequiredService<IOptions<RateLimitOptions>>().Value;
                var principal = context.User.FindFirst("sub")?.Value ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(principal, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limits.PermitLimit,
                    Window = limits.Window,
                    QueueLimit = 0,
                });
            });
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                await Endpoints.ResultHttp.ToProblem(PlatformProblems.RateLimited.With()).ExecuteAsync(context.HttpContext);
            };
        });

        return services;
    }

    private static string CurrentTraceId() =>
        (Activity.Current?.TraceId ?? ActivityTraceId.CreateRandom()).ToHexString();
}

public sealed class RateLimitOptions
{
    public const string Section = "RateLimits";

    public int PermitLimit { get; set; }

    public TimeSpan Window { get; set; }
}
