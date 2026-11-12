using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using Platform.Application.Behaviors;

namespace Api.Telemetry;

/// <summary>
/// Tracing, and the trust boundary it starts at. Every caller of this host is outside the boundary: a request starts its own trace,
/// and a caller's <c>traceparent</c> becomes a link, never the parent.
/// </summary>
public static class TraceBoundary
{
    public static WebApplicationBuilder AddTracing(this WebApplicationBuilder builder)
    {
        // ASP.NET Core starts each request's trace from this propagator, which adopts nothing a caller sends.
        builder.Services.AddSingleton<DistributedContextPropagator>(new NewTracePropagator());

        // OpenTelemetry's ASP.NET Core instrumentation extracts the caller's context itself unless its propagator is plain W3C.
        Sdk.SetDefaultTextMapPropagator(new TraceContextPropagator());

        var telemetry = builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddSource(DispatchTelemetry.SourceName));
        if (builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] is { Length: > 0 })
        {
            telemetry.UseOtlpExporter();
        }

        return builder;
    }

    /// <summary>First in the pipeline: links the request's own trace to the caller's, when the caller sent one.</summary>
    public static IApplicationBuilder UseTraceBoundary(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var headers = context.Request.Headers;
            if (Activity.Current is { } activity
                && ActivityContext.TryParse(headers.TraceParent, headers.TraceState, out var caller))
            {
                activity.AddLink(new ActivityLink(caller));
            }

            await next(context);
        });

    /// <summary>Starts every request's trace afresh; what this host sends onward carries its own context, in W3C form.</summary>
    private sealed class NewTracePropagator : DistributedContextPropagator
    {
        private static readonly DistributedContextPropagator Outgoing = CreateW3CPropagator();

        public override IReadOnlyCollection<string> Fields => Outgoing.Fields;

        public override void Inject(Activity? activity, object? carrier, PropagatorSetterCallback? setter) =>
            Outgoing.Inject(activity, carrier, setter);

        public override void ExtractTraceIdAndState(object? carrier, PropagatorGetterCallback? getter, out string? traceId, out string? traceState)
        {
            traceId = null;
            traceState = null;
        }

        public override IEnumerable<KeyValuePair<string, string?>>? ExtractBaggage(object? carrier, PropagatorGetterCallback? getter) => null;
    }
}
