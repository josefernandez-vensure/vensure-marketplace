using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Behaviors;

namespace Platform.Infrastructure.Messaging;

/// <summary>How often the outbox is read, and how much of it at once.</summary>
internal sealed class OutboxOptions
{
    public const string Section = "Outbox";

    public TimeSpan PollInterval { get; set; }

    public int BatchSize { get; set; }
}

/// <summary>Delivers the outbox in the background: a pass on every tick, repeated while a pass still delivers something.</summary>
internal sealed class OutboxDelivery(OutboxDispatcher dispatcher, IOptions<OutboxOptions> options, TimeProvider time, ILogger<OutboxDelivery> logger)
    : BackgroundService
{
    [SuppressMessage("Design", "CA1031", Justification = "A delivery boundary: a database that can't be reached stops this pass, not the host; the next tick retries.")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.PollInterval, time);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                while (await dispatcher.DeliverPending(options.Value.BatchSize, stoppingToken) > 0)
                {
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                DispatchTelemetry.DeliveryPassFailed(logger, exception);
            }
        }
    }
}
