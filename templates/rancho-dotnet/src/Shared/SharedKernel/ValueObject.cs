namespace SharedKernel;

/// <summary>
/// A value: immutable, validated in its constructor, and equal to another of its type when their components are equal.
/// A derived type must name its components, so its equality can't be forgotten. Unlike a record, it has no <c>with</c>
/// that copies a value past its constructor's validation.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    public static bool operator ==(ValueObject? left, ValueObject? right) => left is null ? right is null : left.Equals(right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);

    public bool Equals(ValueObject? other) =>
        other is not null && other.GetType() == GetType() && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    /// <summary>The values equality compares, in order.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();
}
