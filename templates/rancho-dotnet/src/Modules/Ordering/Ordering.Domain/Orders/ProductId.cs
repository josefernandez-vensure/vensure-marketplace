using SharedKernel;

namespace Ordering.Domain.Orders;

/// <summary>The Catalog product an order is for. An order holds the product's id, never the product.</summary>
[Internal]
public readonly record struct ProductId(Guid Value);
