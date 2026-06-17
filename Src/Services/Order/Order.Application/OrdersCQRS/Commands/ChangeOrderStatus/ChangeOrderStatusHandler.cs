using BuildingBlock.Exceptions;
using Order.Domain.Exceptions;

namespace Order.Application.OrdersCQRS.Commands.ChangeOrderStatus;

public enum OrderStatusAction { Confirm, Process, Ship, Deliver, Cancel }

public record ChangeOrderStatusCommand(Guid OrderId, OrderStatusAction Action) : ICommand<ChangeOrderStatusResult>;
public record ChangeOrderStatusResult(bool IsSuccess);

public class ChangeOrderStatusHandler(IApplicationDbContext dbcontext)
    : ICommandHandler<ChangeOrderStatusCommand, ChangeOrderStatusResult>
{
    public async Task<ChangeOrderStatusResult> Handle(ChangeOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await dbcontext.Orders.FindAsync([OrderId.Of(command.OrderId)], cancellationToken)
            ?? throw new OrderNotFoundException(command.OrderId);

        try
        {
            switch (command.Action)
            {
                case OrderStatusAction.Confirm: order.Confirm(); break;
                case OrderStatusAction.Process: order.Process(); break;
                case OrderStatusAction.Ship: order.Ship(); break;
                case OrderStatusAction.Deliver: order.Deliver(); break;
                case OrderStatusAction.Cancel: order.Cancel(); break;
            }
        }
        catch (DomainException ex)
        {
            // Invalid lifecycle transition -> 400 rather than 500.
            throw new BadRequestException(ex.Message);
        }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return new ChangeOrderStatusResult(true);
    }
}
