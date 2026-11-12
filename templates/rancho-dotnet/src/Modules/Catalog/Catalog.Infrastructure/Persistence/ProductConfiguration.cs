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

        // Tenancy is a scope, not a domain concept: the repository sets it on a new product and every read filters by it.
        builder.Property<Guid>(ProductRepository.TenantColumn);
        builder.HasIndex(ProductRepository.TenantColumn);
        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.Available);

        builder.OwnsMany(p => p.Reservations, reservation =>
        {
            reservation.ToTable("ProductReservations");
            reservation.WithOwner().HasForeignKey("ProductId");
            reservation.Property(r => r.Id).HasConversion(id => id.Value, value => new ReservationId(value)).ValueGeneratedNever();
            reservation.HasKey("ProductId", nameof(StockReservation.Id));
        });
        builder.Navigation(p => p.Reservations).HasField("_reservations").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
