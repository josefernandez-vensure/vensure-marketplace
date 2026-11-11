using Catalog.Contracts;
using Mediator;

namespace Catalog.Application.GetProduct;

public sealed record GetProduct(ProductId ProductId) : IQuery<ProductResponse?>;
