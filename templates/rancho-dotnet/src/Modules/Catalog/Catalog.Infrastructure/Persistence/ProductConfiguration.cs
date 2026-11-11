using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasConversion(id => id.Value, value => new ProductId(value));
        builder.Property(p => p.Name)
            .HasConversion(name => name.Value, value => new ProductName(value))
            .HasMaxLength(ProductName.MaxLength);
        builder.Property(p => p.Version).IsConcurrencyToken();
        builder.Ignore(p => p.DomainEvents);
    }
}
