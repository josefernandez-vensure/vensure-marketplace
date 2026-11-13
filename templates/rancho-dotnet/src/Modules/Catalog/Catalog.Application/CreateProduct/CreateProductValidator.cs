using Catalog.Domain.Products;
using FluentValidation;

namespace Catalog.Application.CreateProduct;

public sealed class CreateProductValidator : AbstractValidator<CreateProduct>
{
    public CreateProductValidator()
    {
        RuleFor(c => c.ProductId.Value).NotEmpty().OverridePropertyName("productId");
        RuleFor(c => c.Name).NotEmpty().MaximumLength(ProductName.MaxLength);
    }
}
