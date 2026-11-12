using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Platform.Application.Behaviors;

public static partial class DispatchTelemetry
{
    public const string SourceName = "Platform.Dispatch";

    public static readonly ActivitySource Source = new(SourceName);

    /// <summary>The tag naming the pipeline step a span measures: the behavior's type name.</summary>
    public const string StepTag = "dispatch.step";

    /// <summary>A span for one step of the pipeline, nested in the step before it, so each step's time is separable.</summary>
    public static Activity? StartStep(string name, Type step)
    {
        var activity = Source.StartActivity(name);
        activity?.SetTag(StepTag, StepName(step));
        return activity;
    }

    public static string StepName(Type step) => step.Name.Split('`')[0];

    [LoggerMessage(Level = LogLevel.Information, Message = "{Operation} succeeded in {ElapsedMs} ms")]
    public static partial void Succeeded(ILogger logger, [Internal] string operation, [Internal] double elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Operation} failed with {ProblemType} in {ElapsedMs} ms")]
    public static partial void Failed(ILogger logger, [Internal] string operation, [Internal] string problemType, [Internal] double elapsedMs);
}
