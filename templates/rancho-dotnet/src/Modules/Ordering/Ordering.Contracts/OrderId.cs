using SharedKernel;

namespace Ordering.Contracts;

/// <summary>The wire id of an order. Messages and DTOs carry this, never the domain id.</summary>
[Internal]
public readonly record struct OrderId(Guid Value);
