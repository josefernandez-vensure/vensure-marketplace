using Platform.Application.Abstractions;

namespace Catalog.Contracts.IntegrationEvents;

/// <summary>The stock could not be held: the product is unknown or discontinued, or too little of it is available.</summary>
public sealed record StockReservationFailedV1(ReservationId ReservationId, ProductId ProductId) : IIntegrationEvent;
