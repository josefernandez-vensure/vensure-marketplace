namespace SharedKernel;

public abstract class AggregateRoot<TId> : IAggregateRoot
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TId Id { get; protected set; }

    /// <summary>The concurrency token and the ETag. It increments on every change.</summary>
    public long Version { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
        Version++;
    }

    protected void Touch() => Version++;
}
