using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Shouldly;

namespace Api.Tests;

/// <summary>Every caller is outside the trust boundary: its trace is linked, never adopted.</summary>
public sealed class TraceBoundaryTests
{
    [Fact]
    public async Task A_callers_traceparent_starts_a_new_trace_linked_to_the_callers()
    {
        var callerTrace = ActivityTraceId.CreateRandom();
        var callerSpan = ActivitySpanId.CreateRandom();
        var requests = new ConcurrentBag<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Microsoft.AspNetCore",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = requests.Add,
        };
        ActivitySource.AddActivityListener(listener);
        using var factory = new ApiFactory();

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/catalog/products/{Guid.NewGuid()}");
        request.Headers.TryAddWithoutValidation("traceparent", $"00-{callerTrace.ToHexString()}-{callerSpan.ToHexString()}-01");
        var response = await factory.CreateClient().SendAsync(request, TestContext.Current.CancellationToken);

        // The answer is the anonymous caller's refusal; its traceId is the trace the host started.
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var traceId = body.RootElement.GetProperty("traceId").GetString();
        traceId.ShouldNotBe(callerTrace.ToHexString());

        var server = requests.SingleOrDefault(a => a.TraceId.ToHexString() == traceId).ShouldNotBeNull("the host traced the request under the trace it answered with");
        server.Links.Select(l => l.Context.TraceId).ShouldContain(callerTrace);
    }
}
