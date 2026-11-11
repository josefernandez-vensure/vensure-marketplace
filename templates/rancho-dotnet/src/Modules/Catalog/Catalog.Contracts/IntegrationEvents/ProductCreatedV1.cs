using Platform.Application.Abstractions;

namespace Catalog.Contracts.IntegrationEvents;

public sealed record ProductCreatedV1(ProductId ProductId, DateTimeOffset At) : IIntegrationEvent;
