using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Orders;

namespace Ordering.Infrastructure.Persistence;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasConversion(id => id.Value, value => new OrderId(value));
        builder.Property(o => o.ProductId).HasConversion(id => id.Value, value => new ProductId(value));
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Version).IsConcurrencyToken();

        // Tenancy is a scope, not a domain concept: the repository sets it on a new order and every read filters by it.
        builder.Property<Guid>(OrderRepository.TenantColumn);
        builder.HasIndex(OrderRepository.TenantColumn);
        builder.Ignore(o => o.DomainEvents);
    }
}
