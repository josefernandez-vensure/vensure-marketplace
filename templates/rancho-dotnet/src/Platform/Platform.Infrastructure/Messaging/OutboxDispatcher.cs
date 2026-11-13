using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Platform.Application.Abstractions;
using Platform.Application.Behaviors;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Messaging;

/// <summary>
/// Delivers every module's outbox. Each message is delivered under the trace context of the transaction that wrote it, to every reaction
/// registered for its type: each reaction's command runs in a scope of its own, as its module's service principal, in the message's tenant,
/// keyed by the message's id. A message is marked delivered only when every reaction succeeded, so delivery is at least once.
/// </summary>
internal sealed class OutboxDispatcher(OpenDeliveryScope open, ReactionMap reactions, TimeProvider time, ILogger<OutboxDispatcher> logger)
{
    /// <summary>One pass: up to <paramref name="batchSize"/> undelivered messages from each module's outbox, oldest first.</summary>
    /// <returns>How many messages this pass delivered.</returns>
    public async Task<int> DeliverPending(int batchSize, CancellationToken cancellationToken)
    {
        await using var reading = open(null);
        var delivered = 0;
        foreach (var outbox in reading.Outboxes)
        {
            var pending = await outbox.Set<OutboxMessage>().AsNoTracking()
                .Where(m => m.DeliveredAt == null)
                .OrderBy(m => m.OccurredAt).ThenBy(m => m.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            foreach (var message in pending)
            {
                if (!await Deliver(message, cancellationToken))
                {
                    continue;
                }

                var now = time.GetUtcNow();
                await outbox.Set<OutboxMessage>()
                    .Where(m => m.Id == message.Id)
                    .ExecuteUpdateAsync(set => set.SetProperty(m => m.DeliveredAt, now), cancellationToken);
                delivered++;
            }
        }

        return delivered;
    }

    private async Task<bool> Deliver(OutboxMessage message, CancellationToken cancellationToken)
    {
        var answering = reactions.For(message.Type);
        if (answering.Count == 0)
        {
            return true;
        }

        using var delivery = Restore(message);
        foreach (var reaction in answering)
        {
            if (!await React(reaction, message, delivery.TraceId.ToHexString(), cancellationToken))
            {
                return false;
            }
        }

        return true;
    }

    [SuppressMessage("Design", "CA1031", Justification = "A delivery boundary: a reaction that throws leaves its message undelivered for the next pass, and the other messages are still delivered.")]
    private async Task<bool> React(Reaction reaction, OutboxMessage message, string traceId, CancellationToken cancellationToken)
    {
        var context = new DispatchContext(
            ServicePrincipal.For(reaction.Module),
            ServicePrincipal.IdOf(reaction.Module),
            traceId,
            message.TenantId is { } tenant ? new TenantId(tenant) : null,
            idempotencyKey: message.Id.ToString(),
            causation: new Causation(message.Id, message.InitiatingPrincipalId));
        try
        {
            await using var scope = open(context);
            var command = reaction.Map(JsonSerializer.Deserialize(message.Payload, reaction.MessageType)!);
            var result = (IDispatchResult)(await scope.Sender.Send(command, cancellationToken))!;
            if (result.IsSuccess)
            {
                return true;
            }

            DispatchTelemetry.ReactionFailed(logger, message.Id, reaction.Name, result.Problem!.Type.Uri);
            return false;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            DispatchTelemetry.ReactionThrew(logger, exception, message.Id, reaction.Name);
            return false;
        }
    }

    /// <summary>
    /// The delivery's span, a child of the span that wrote the message, so the trace that began with the write continues here.
    /// With no listener sampling it, the context is still restored, so the reaction's logs and audit carry the writer's trace id.
    /// </summary>
    private static Activity Restore(OutboxMessage message)
    {
        if (!ActivityContext.TryParse(message.TraceParent, message.TraceState, isRemote: true, out var writer))
        {
            writer = new ActivityContext(ActivityTraceId.CreateRandom(), ActivitySpanId.CreateRandom(), ActivityTraceFlags.None);
        }

        var activity = DispatchTelemetry.Source.StartActivity(DispatchTelemetry.DeliverSpan, ActivityKind.Consumer, writer)
            ?? new Activity(DispatchTelemetry.DeliverSpan).SetParentId(writer.TraceId, writer.SpanId, writer.TraceFlags).Start();
        activity.TraceStateString = message.TraceState;
        activity.SetTag("messaging.message.id", message.Id.ToString());
        activity.SetTag("messaging.message.type", message.Type);
        return activity;
    }
}
