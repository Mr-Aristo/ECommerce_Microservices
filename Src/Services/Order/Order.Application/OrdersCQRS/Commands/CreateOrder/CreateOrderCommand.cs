namespace Order.Application.OrdersCQRS.Commands.CreateOrder;


public record CreateOrderCommand(OrderDto Order): ICommand<CreateOrderResult>;
public record CreateOrderResult(Guid id);

/// <summary>
/// The CreateOrderCommandValidator class is responsible for validating the CreateOrderCommand.
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
	{
        RuleFor(x => x.Order.OrderName).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Order.CustomerId).NotEmpty().WithMessage("CustomerId is required");
        RuleFor(x => x.Order.OrderItems).NotEmpty().WithMessage("OrderItems should not be empty");
        RuleFor(x => x.Order.ShippingAddress.EmailAddress).NotEmpty().WithMessage("Shipping email is required");
        RuleFor(x => x.Order.ShippingAddress.AddressLine).NotEmpty().WithMessage("Shipping address line is required");
        RuleFor(x => x.Order.BillingAddress.EmailAddress).NotEmpty().WithMessage("Billing email is required");
        RuleFor(x => x.Order.BillingAddress.AddressLine).NotEmpty().WithMessage("Billing address line is required");
        RuleFor(x => x.Order.Payment.CardName).NotEmpty().WithMessage("CardName is required");
        RuleFor(x => x.Order.Payment.CardNumber).NotEmpty().WithMessage("CardNumber is required");
        RuleFor(x => x.Order.Payment.Cvv)
            .MaximumLength(3).WithMessage("CVV must be at most 3 characters");

        RuleForEach(x => x.Order.OrderItems).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("ProductId is required");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
            item.RuleFor(i => i.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
        });
    }
}
