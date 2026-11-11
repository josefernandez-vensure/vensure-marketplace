using Catalog.Contracts;

namespace Catalog.Application.GetProduct;

public sealed record ProductResponse(ProductId Id, string Name, bool IsDiscontinued, string ETag);
