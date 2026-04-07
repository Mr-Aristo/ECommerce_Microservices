namespace Order.Application.OrdersCQRS.Commands.DeleteOrder;

public record DeleteOrderCommand(Guid OrderId) : ICommand<DeleteOrderResult>;

public record DeleteOrderResult(bool IsSuccess);

/// <summary>
/// The DeleteOrderValidator class is responsible for validating the DeleteOrderCommand.
///  It ensures that the OrderId property of the command is not empty, which is essential 
/// for identifying the order to be deleted. If the validation fails, it provides a meaningful 
/// error message indicating that the OrderId is required.
/// </summary>
public class DeleteOrderValidator : AbstractValidator<DeleteOrderCommand>
{
	public DeleteOrderValidator()
	{
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required");
    }
}

