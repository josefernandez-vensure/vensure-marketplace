using Platform.Application.Abstractions;
using SharedKernel;

namespace Catalog.Contracts.IntegrationEvents;

public sealed record ProductCreatedV1(ProductId ProductId, [Internal] DateTimeOffset At) : IIntegrationEvent;
