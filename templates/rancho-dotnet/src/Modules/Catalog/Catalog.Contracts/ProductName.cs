using SharedKernel;

namespace Catalog.Contracts;

/// <summary>A product's name as the module publishes it. A member projected from a name takes its class from here.</summary>
[Public]
public sealed record ProductName(string Value);
