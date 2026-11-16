using Catalog.Contracts;
using Platform.Application.Abstractions;

namespace Catalog.Application.RenameProducts;

/// <summary>Corrects several products' names in one transaction, all or none. It carries no If-Match; a race lost at commit answers 409.</summary>
public sealed record RenameProducts(IReadOnlyList<ProductRename> Products) : ICommand, IMultiAggregateCommand
{
    public const int MaxProducts = 100;

    public static Operation Operation { get; } =
        Operation.Command("catalog.rename-products", CatalogPolicies.Write, IdempotencyMechanism.Natural);
}

public sealed record ProductRename(ProductId ProductId, string Name);
