using SharedKernel;

namespace Catalog.Domain.Products;

[Public]
public sealed class ProductName : ValueObject
{
    public const int MaxLength = 200;

    public ProductName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var trimmed = value.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, MaxLength, nameof(value));
        Value = trimmed;
    }

    public string Value { get; }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
