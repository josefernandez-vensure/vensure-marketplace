namespace SharedKernel;

/// <summary>Marks an aggregate root. A repository serves only roots.</summary>
public interface IAggregateRoot
{
    long Version { get; }
}
