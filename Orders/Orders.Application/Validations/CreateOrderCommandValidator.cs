using FluentValidation;
using Orders.Application.Commands.CreateOrder;

namespace Orders.Application.Validations;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    private static readonly string[] ValidPaymentMethods = ["nequi", "daviplata", "mercado pago"];

    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.IdAddress)
            .NotEmpty().WithMessage("Address is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.JerseyId)
                .GreaterThan(0).WithMessage("Invalid jersey ID");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            item.RuleFor(i => i.Size)
                .NotEmpty().WithMessage("Size is required");

            item.RuleFor(i => i.Name)
                .NotEmpty().WithMessage("Item name is required");
        });

        RuleFor(x => x.Payment.Method)
            .NotEmpty().WithMessage("Payment method is required")
            .Must(m => ValidPaymentMethods.Contains(m))
            .WithMessage($"Payment method must be one of: {string.Join(", ", ValidPaymentMethods)}");

        RuleFor(x => x.Payment.AccountId)
            .NotEmpty().WithMessage("Payment account ID is required");

        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("Shipping cost must be >= 0");
    }
}
