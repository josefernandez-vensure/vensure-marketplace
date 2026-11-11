namespace SharedKernel;

/// <summary>The base of a module's repository port: one per aggregate root, whole aggregates only.</summary>
public interface IRepository<TRoot, in TId>
    where TRoot : IAggregateRoot
    where TId : struct
{
    Task<TRoot?> Find(TId id, CancellationToken cancellationToken);

    /// <summary>Stages a new root, or a loaded one's changes, for the unit of work. The pipeline commits; a handler never does.</summary>
    void Save(TRoot root);
}
