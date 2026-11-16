using Catalog.Domain.Products;
using FluentValidation;

namespace Catalog.Application.RenameProducts;

public sealed class RenameProductsValidator : AbstractValidator<RenameProducts>
{
    public RenameProductsValidator()
    {
        RuleFor(c => c.Products).NotEmpty().Must(p => p.Count <= RenameProducts.MaxProducts)
            .WithMessage($"At most {RenameProducts.MaxProducts} products are renamed at once.");
        RuleFor(c => c.Products).Must(p => p.Select(x => x.ProductId).Distinct().Count() == p.Count)
            .WithMessage("Each product is named once.");
        RuleForEach(c => c.Products).ChildRules(p => p.RuleFor(x => x.Name).NotEmpty().MaximumLength(ProductName.MaxLength));
    }
}
