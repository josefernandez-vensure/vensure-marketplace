using Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Queries.ListProducts;

/// <summary>A keyless read type over the products table: queries read it untracked, and migrations ignore it.</summary>
internal sealed class ProductSummaryRow
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }
}

internal sealed class ProductSummaryRowConfiguration : IEntityTypeConfiguration<ProductSummaryRow>
{
    public void Configure(EntityTypeBuilder<ProductSummaryRow> builder)
    {
        builder.HasNoKey();
        builder.ToView("Products", CatalogDbContext.SchemaName);
    }
}
