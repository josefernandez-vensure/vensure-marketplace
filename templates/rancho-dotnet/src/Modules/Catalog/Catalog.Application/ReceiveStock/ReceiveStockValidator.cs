using FluentValidation;

namespace Catalog.Application.ReceiveStock;

public sealed class ReceiveStockValidator : AbstractValidator<ReceiveStock>
{
    public ReceiveStockValidator()
    {
        RuleFor(c => c.Quantity).GreaterThan(0);
    }
}
