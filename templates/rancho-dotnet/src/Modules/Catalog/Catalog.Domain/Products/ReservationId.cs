using SharedKernel;

namespace Catalog.Domain.Products;

[Internal]
public readonly record struct ReservationId(Guid Value);
