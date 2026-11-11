namespace SharedKernel;

public abstract class AggregateRoot<TId> : IAggregateRoot
    where TId : struct
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public TId Id { get; protected set; }

    public long Version { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
        Version++;
    }

    protected void Touch() => Version++;
}
