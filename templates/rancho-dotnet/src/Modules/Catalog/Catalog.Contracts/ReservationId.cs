using SharedKernel;

namespace Catalog.Contracts;

/// <summary>The wire id of a stock reservation. The process that asks for one names it, so its replies find their way back.</summary>
[Internal]
public readonly record struct ReservationId(Guid Value);
