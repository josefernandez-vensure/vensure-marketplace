using Platform.Application.Abstractions;
using SharedKernel;

namespace Catalog.Contracts.IntegrationEvents;

public sealed record StockReservedV1(ReservationId ReservationId, ProductId ProductId, [Internal] int Quantity) : IIntegrationEvent;
