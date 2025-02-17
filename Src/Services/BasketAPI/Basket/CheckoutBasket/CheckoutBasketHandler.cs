namespace BasketAPI.Basket.CheckoutBasket;

/** Recods **/
public record CheckoutBasketCommand(BasketCheckoutDto BasketCheckoutDto) : ICommand<CheckoutBasketResult>;
public record CheckoutBasketResult(bool IsSuccess);
/***********/

/** Checkout Validation **/
public abstract class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand> //FluentValidation
{
	public CheckoutBasketCommandValidator()
	{
        //x."variableName" must be the same as AbstractValidator<"Type's parameters name which is CheckoutBasketCommand">
        RuleFor(x => x.BasketCheckoutDto).NotNull().WithMessage("BasketCheckoutDto cannot be null.");

		RuleFor(x => x.BasketCheckoutDto.UserName).NotEmpty().WithMessage("UserName is required.");
	}
}
/*************************/

public class CheckoutBasketHandler(IBasketRepository repository /*, IPublishEndpoint publishEndpoint*/) : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasket(command.BasketCheckoutDto.FirstName, cancellationToken);

        if (basket is null)
        {
            return new CheckoutBasketResult(false);
        }

        //var eventMessage = command.BasketCheckoutDto.Adapt<BasketCheckoutEvent>();
        //eventMessage.TotalPrice = basket.TotalPrice;

        //await publishEndpoint.Publish(eventMessage, cancellationToken);

        await repository.DeleteBasket(command.BasketCheckoutDto.UserName, cancellationToken);

        return new CheckoutBasketResult(true);
    }
}
