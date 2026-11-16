using FluentValidation;

namespace Catalog.Application.ListProducts;

public sealed class ListProductsValidator : AbstractValidator<ListProducts>
{
    public ListProductsValidator()
    {
        RuleFor(q => q.Sort).IsInEnum();
    }
}
