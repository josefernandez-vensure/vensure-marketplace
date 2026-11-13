using FluentValidation;

namespace Ordering.Application.PlaceOrder;

public sealed class PlaceOrderValidator : AbstractValidator<PlaceOrder>
{
    public const int MaxQuantity = 1000;

    public PlaceOrderValidator()
    {
        RuleFor(c => c.OrderId.Value).NotEmpty().OverridePropertyName("id");
        RuleFor(c => c.ProductId.Value).NotEmpty().OverridePropertyName("productId");
        RuleFor(c => c.Quantity).InclusiveBetween(1, MaxQuantity);
    }
}
