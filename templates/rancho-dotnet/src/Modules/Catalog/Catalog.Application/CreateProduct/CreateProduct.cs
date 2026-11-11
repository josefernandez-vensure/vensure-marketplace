using Catalog.Contracts;
using Mediator;

namespace Catalog.Application.CreateProduct;

public sealed record CreateProduct(string Name) : ICommand<ProductId>;
