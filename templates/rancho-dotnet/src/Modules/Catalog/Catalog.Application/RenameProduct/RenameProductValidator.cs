using Catalog.Domain.Products;
using FluentValidation;

namespace Catalog.Application.RenameProduct;

public sealed class RenameProductValidator : AbstractValidator<RenameProduct>
{
    public RenameProductValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(ProductName.MaxLength);
    }
}
