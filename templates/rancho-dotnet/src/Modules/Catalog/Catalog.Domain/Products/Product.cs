using SharedKernel;

namespace Catalog.Domain.Products;

public sealed class Product : AggregateRoot<ProductId>
{
    private Product(ProductId id, ProductName name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
    }

    public ProductName Name { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsDiscontinued { get; private set; }

    public static Product Create(ProductId id, ProductName name, DateTimeOffset now)
    {
        var product = new Product(id, name, now);
        product.Raise(new ProductCreated(id, now));
        return product;
    }

    public RenameOutcome Rename(ProductName name)
    {
        if (IsDiscontinued)
        {
            return RenameOutcome.Discontinued;
        }

        if (name == Name)
        {
            return RenameOutcome.Unchanged;
        }

        Name = name;
        Touch();
        return RenameOutcome.Renamed;
    }

    public void Discontinue()
    {
        if (IsDiscontinued)
        {
            return;
        }

        IsDiscontinued = true;
        Touch();
    }
}
