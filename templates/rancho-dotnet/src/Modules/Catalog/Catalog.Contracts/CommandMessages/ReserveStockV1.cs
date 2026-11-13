using Platform.Application.Abstractions;
using SharedKernel;

namespace Catalog.Contracts.CommandMessages;

/// <summary>Asks Catalog to hold stock for a process in flight. Catalog answers with <c>StockReservedV1</c> or <c>StockReservationFailedV1</c>.</summary>
public sealed record ReserveStockV1(ReservationId ReservationId, ProductId ProductId, [Internal] int Quantity) : ICommandMessage;
