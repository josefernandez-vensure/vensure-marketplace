using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Api.Telemetry;

/// <summary>
/// Keeps an exception's message in a trace only where our code wrote it: where the method that threw is in one of this service's assemblies
/// (those that declare a ProjectKind). Any other message, such as a driver's, which can quote the values it was given, is dropped before
/// the exception becomes a span event, and so is every message in its stack trace. The type and the frames stay.
/// </summary>
public static class ExceptionScrubbing
{
    public const string Dropped = "(message dropped: not written by this service)";

    private static readonly Lazy<ActivityListener> Listener = new(Start);

    /// <summary>Listens to every source, samples nothing itself, and writes the exception event's tags before the runtime would.</summary>
    public static void EnsureStarted() => _ = Listener.Value;

    /// <summary>Whether this service's own code threw the exception, and so wrote its message.</summary>
    public static bool WrittenByUs(Exception exception) =>
        new StackTrace(exception, fNeedFileInfo: false).GetFrame(0)?.GetMethod()?.DeclaringType?.Assembly is { } thrower
        && thrower.GetCustomAttributes<AssemblyMetadataAttribute>().Any(a => a.Key == "ProjectKind" && !string.IsNullOrEmpty(a.Value));

    private static ActivityListener Start()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.None,
            ExceptionRecorder = Record,
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    private static void Record(Activity activity, Exception exception, ref TagList tags)
    {
        tags.Add("exception.message", WrittenByUs(exception) ? exception.Message : Dropped);
        tags.Add("exception.stacktrace", Describe(exception));
    }

    /// <summary>Like <see cref="Exception.ToString"/>, with each message kept only where our code wrote it.</summary>
    private static string Describe(Exception exception)
    {
        var text = new StringBuilder(exception.GetType().FullName);
        text.Append(": ").Append(WrittenByUs(exception) ? exception.Message : Dropped);
        if (exception.InnerException is { } inner)
        {
            text.Append(" ---> ").Append(Describe(inner)).Append(Environment.NewLine).Append("   --- End of inner exception stack trace ---");
        }

        if (exception.StackTrace is { } frames)
        {
            text.Append(Environment.NewLine).Append(frames);
        }

        return text.ToString();
    }
}
