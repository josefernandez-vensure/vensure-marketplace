using System.Diagnostics;
using System.Globalization;
using Api.Telemetry;
using Platform.Application.Abstractions;
using Shouldly;

namespace Api.Tests;

/// <summary>Core §3: telemetry keeps an exception's message only where our code wrote it.</summary>
public sealed class ExceptionScrubbingTests
{
    [Fact]
    public void A_message_our_code_did_not_write_never_reaches_a_span()
    {
        // The host's tracing setup is what starts the scrubbing.
        using var factory = new ApiFactory();
        _ = factory.Server;
        using var source = new ActivitySource("Api.Tests.Scrubbing");
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s == source,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(listener);

        // The runtime's message quotes the value it was given; ours names what is wrong.
        var foreign = Thrown(() => int.Parse("4111-1111-1111-1111", CultureInfo.InvariantCulture));
        var ours = Thrown(() => Operation.Command("tests.scrub", "tests.scrub", IdempotencyMechanism.Key, TransactionRequirement.None));
        foreign.Message.ShouldContain("4111");

        using var activity = source.StartActivity("work")!;
        activity.AddException(foreign);
        activity.AddException(ours);

        var events = activity.Events.Where(e => e.Name == "exception").ToList();
        events.Count.ShouldBe(2);
        Tag(events[0], "exception.message").ShouldBe(ExceptionScrubbing.Dropped);
        Tag(events[0], "exception.stacktrace").ShouldNotBeNull().ShouldNotContain("4111");
        Tag(events[0], "exception.type").ShouldBe(typeof(FormatException).FullName);
        Tag(events[1], "exception.message").ShouldBe(ours.Message);
    }

    private static string? Tag(ActivityEvent e, string name) => e.Tags.FirstOrDefault(t => t.Key == name).Value as string;

    private static Exception Thrown(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception) when (exception is FormatException or ArgumentException)
        {
            return exception;
        }

        throw new InvalidOperationException("Expected the action to throw.");
    }
}
