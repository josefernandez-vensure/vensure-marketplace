using SharedKernel;

namespace Ordering.Domain.Orders;

[Internal]
public readonly record struct OrderId(Guid Value);
