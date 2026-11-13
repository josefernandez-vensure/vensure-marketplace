using Mediator;
using Platform.Application.Abstractions;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Messaging;

/// <summary>
/// The dispatch context a scope was opened with, when no request supplies one. Only the outbox dispatcher enters it, once per scope,
/// before anything in the scope resolves the context.
/// </summary>
internal sealed class DispatchScope
{
    public DispatchContext? Context { get; private set; }

    public void Enter(DispatchContext context)
    {
        if (Context is not null)
        {
            throw new InvalidOperationException("A scope dispatches under one context.");
        }

        Context = context;
    }
}

/// <summary>A dependency scope for one step of delivery: reading the outboxes, or dispatching one reaction under its own context.</summary>
internal sealed class DeliveryScope(IAsyncDisposable scope, IEnumerable<ModuleDbContext> outboxes, ISender sender) : IAsyncDisposable
{
    public IEnumerable<ModuleDbContext> Outboxes { get; } = outboxes;

    public ISender Sender { get; } = sender;

    public ValueTask DisposeAsync() => scope.DisposeAsync();
}

/// <summary>Opens a scope; the composition root builds it, since only the composition root resolves services.</summary>
internal delegate DeliveryScope OpenDeliveryScope(DispatchContext? context);
