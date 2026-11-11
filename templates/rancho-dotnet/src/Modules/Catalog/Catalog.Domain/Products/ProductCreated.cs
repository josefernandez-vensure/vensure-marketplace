using SharedKernel;

namespace Catalog.Domain.Products;

public sealed record ProductCreated(ProductId ProductId, DateTimeOffset At) : IDomainEvent;
