using SharedKernel;

namespace Catalog.Domain.Products;

[Internal]
public readonly record struct ProductId(Guid Value)
{
    public static ProductId New() => new(Guid.CreateVersion7());
}
