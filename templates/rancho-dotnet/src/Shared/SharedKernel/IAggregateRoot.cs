namespace SharedKernel;

/// <summary>Marks an aggregate root. A repository serves only roots.</summary>
public interface IAggregateRoot
{
    /// <summary>The concurrency token and the ETag. It increments on every change.</summary>
    long Version { get; }

    /// <summary>Events raised since the root was loaded. The unit of work writes them to the outbox with the change.</summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>Called by the unit of work once the events are staged in the outbox.</summary>
    void ClearDomainEvents();
}
