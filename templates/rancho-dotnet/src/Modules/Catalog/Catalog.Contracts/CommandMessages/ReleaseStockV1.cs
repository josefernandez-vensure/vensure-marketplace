using Platform.Application.Abstractions;

namespace Catalog.Contracts.CommandMessages;

/// <summary>The compensation of <see cref="ReserveStockV1"/>: releases what the reservation holds. Sending it twice changes nothing.</summary>
public sealed record ReleaseStockV1(ReservationId ReservationId, ProductId ProductId) : ICommandMessage;
