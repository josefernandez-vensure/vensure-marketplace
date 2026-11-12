using System.Collections.Concurrent;
using System.Diagnostics;
using Catalog.Application.CreateProduct;
using Catalog.Application.ListProducts;
using Catalog.Contracts;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Platform.Application.Behaviors;
using Shouldly;
using static Catalog.IntegrationTests.Catalog;

namespace Catalog.IntegrationTests;

/// <summary>
/// Every behavior the container resolves for a message emits a span, nested in pipeline order, and the last one wraps the
/// handler alone: authorization, validation and handler time are separable in a trace.
/// </summary>
public sealed class PipelineTracingTests(SqlServer sql)
{
    [Fact]
    public async Task Every_step_of_a_command_emits_a_span_nested_in_pipeline_order() =>
        await ShouldTrace<CreateProduct, Result<ProductId>>(new CreateProduct(new ProductId(Guid.CreateVersion7()), "Traced lamp"));

    [Fact]
    public async Task Every_step_of_a_paged_query_emits_a_span_nested_in_pipeline_order() =>
        await ShouldTrace<ListProducts, Result<Page<ProductSummary>>>(new ListProducts("Traced"));

    private async Task ShouldTrace<TMessage, TResponse>(TMessage message)
        where TMessage : IMessage
        where TResponse : IResult<TResponse>
    {
        var spans = new ConcurrentBag<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == DispatchTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = spans.Add,
        };
        ActivitySource.AddActivityListener(listener);
        using var source = new ActivitySource("Catalog.IntegrationTests");
        using var testListener = new ActivityListener
        {
            ShouldListenTo = s => s == source,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(testListener);

        using var scope = sql.Api.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext { User = CatalogApi.Principal("tracer", "catalog.read", "catalog.write") };
        var expected = scope.ServiceProvider.GetServices<IPipelineBehavior<TMessage, TResponse>>()
            .Select(b => DispatchTelemetry.StepName(b.GetType()))
            .ToList();

        using (var root = source.StartActivity("test")!)
        {
            var result = (TResponse)(await scope.ServiceProvider.GetRequiredService<ISender>().Send(message, Cancel))!;
            result.IsSuccess.ShouldBeTrue();

            var mine = spans.Where(s => s.TraceId == root.TraceId).ToList();
            var chain = new List<string>();
            for (var parent = root.SpanId; mine.SingleOrDefault(s => s.ParentSpanId == parent) is { } step; parent = step.SpanId)
            {
                chain.Add(step.GetTagItem(DispatchTelemetry.StepTag) as string ?? step.DisplayName);
            }

            expected.ShouldNotBeEmpty();
            expected[^1].ShouldBe(DispatchTelemetry.StepName(typeof(HandlerSpanBehavior<,>)));
            chain.ShouldBe(expected, "each behavior's span is the child of the one before it, and the handler's span is last");
        }
    }
}
