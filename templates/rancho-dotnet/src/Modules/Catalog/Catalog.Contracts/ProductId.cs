using SharedKernel;

namespace Catalog.Contracts;

/// <summary>The wire id of a product. Messages and DTOs carry this, never the domain id.</summary>
[Internal]
public readonly record struct ProductId(Guid Value);
