using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Platform.Application.Behaviors;

public static partial class DispatchTelemetry
{
    public const string SourceName = "Platform.Dispatch";

    public static readonly ActivitySource Source = new(SourceName);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Operation} succeeded in {ElapsedMs} ms")]
    public static partial void Succeeded(ILogger logger, string operation, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Operation} failed with {ProblemType} in {ElapsedMs} ms")]
    public static partial void Failed(ILogger logger, string operation, string problemType, double elapsedMs);
}
