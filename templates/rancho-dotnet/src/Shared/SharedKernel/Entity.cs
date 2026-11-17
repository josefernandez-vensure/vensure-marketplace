namespace SharedKernel;

/// <summary>
/// An object with identity: its id, not its values, makes it itself, and its state changes over its life.
/// An aggregate root is one; a child entity is one that lives inside its root and is reached only through it.
/// </summary>
public abstract class Entity<TId>
    where TId : struct
{
    public TId Id { get; protected set; }
}
