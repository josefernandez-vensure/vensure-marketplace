namespace SharedKernel;

/// <summary>The base of a module's repository port: one per aggregate root, whole aggregates only.</summary>
public interface IRepository<TRoot, in TId>
    where TRoot : IAggregateRoot
    where TId : struct
{
    Task<TRoot?> Find(TId id, CancellationToken cancellationToken);

    Task Save(TRoot root, CancellationToken cancellationToken);
}
