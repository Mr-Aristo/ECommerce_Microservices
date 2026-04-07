using BuildingBlock.Exceptions;
using BuildingBlockMessaging.Events;
using MassTransit;

namespace BasketAPI.Basket.CheckoutBasket;

// Command & Handler Records
public record CheckoutBasketCommand(BasketCheckoutDto BasketCheckoutDto) : ICommand<CheckoutBasketResult>;
public record CheckoutBasketResult(bool IsSuccess);
    
//FluentValidation
public class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand> //FluentValidation
{
	public CheckoutBasketCommandValidator()
	{
        //x."variableName" must be the same as AbstractValidator<"Type's parameters name which is CheckoutBasketCommand">
        RuleFor(x => x.BasketCheckoutDto).NotNull().WithMessage("BasketCheckoutDto cannot be null.");
        RuleFor(x => x.BasketCheckoutDto.UserName).NotEmpty().WithMessage("UserName is required.");
        RuleFor(x => x.BasketCheckoutDto.CustomerId).NotEmpty().WithMessage("CustomerId is required.");
        RuleFor(x => x.BasketCheckoutDto.EmailAddress).NotEmpty().WithMessage("EmailAddress is required.");
        RuleFor(x => x.BasketCheckoutDto.AddressLine).NotEmpty().WithMessage("AddressLine is required.");
        RuleFor(x => x.BasketCheckoutDto.CardName).NotEmpty().WithMessage("CardName is required.");
        RuleFor(x => x.BasketCheckoutDto.CardNumber).NotEmpty().WithMessage("CardNumber is required.");
        RuleFor(x => x.BasketCheckoutDto.CVV)
            .MaximumLength(3).WithMessage("CVV must be at most 3 characters.");
	}
}

public class CheckoutBasketHandler(IBasketRepository repository , IPublishEndpoint publishEndpoint) : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasket(command.BasketCheckoutDto.UserName, cancellationToken);

        if (!basket.Items.Any())
        {
            throw new BadRequestException("Checkout cannot proceed with an empty basket.");
        }

        var eventMessage = command.BasketCheckoutDto.Adapt<BasketCheckoutEvent>();
        eventMessage.TotalPrice = basket.TotalPrice;
        eventMessage.CardNumber = MaskCardNumber(command.BasketCheckoutDto.CardNumber);
        eventMessage.CVV = RedactCvv();
        eventMessage.Items = basket.Items
            .Select(item => new BasketCheckoutItemEvent
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                Price = item.Price
            })
            .ToList();

        await publishEndpoint.Publish(eventMessage, cancellationToken); 

        await repository.DeleteBasket(command.BasketCheckoutDto.UserName, cancellationToken);

        return new CheckoutBasketResult(true);
    }

    private static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return "****";
        }

        var digitsOnly = new string(cardNumber.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length == 0)
        {
            return "****";
        }

        var containsMaskChars = cardNumber.Contains('*');
        if (digitsOnly.Length <= 4)
        {
            return containsMaskChars
                ? $"**** **** **** {digitsOnly}"
                : "****";
        }

        var visiblePart = digitsOnly[^4..];
        return $"**** **** **** {visiblePart}";
    }

    private static string RedactCvv() => "***";
}
