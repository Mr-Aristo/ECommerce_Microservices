using BuildingBlock.Exceptions;
using Order.Domain.Exceptions;

namespace Order.Application.OrdersCQRS.Commands.Returns;

public record ReturnResult(bool IsSuccess);

public record RequestReturnCommand(Guid OrderId, Guid CustomerId, string Reason) : ICommand<ReturnResult>;
public record ApproveReturnCommand(Guid OrderId) : ICommand<ReturnResult>;
public record RejectReturnCommand(Guid OrderId, string Reason) : ICommand<ReturnResult>;
public record CompleteRefundCommand(Guid OrderId) : ICommand<ReturnResult>;

public class RequestReturnHandler(IApplicationDbContext dbcontext) : ICommandHandler<RequestReturnCommand, ReturnResult>
{
    // O2: 14-day return window from delivery.
    private static readonly TimeSpan ReturnWindow = TimeSpan.FromDays(14);

    public async Task<ReturnResult> Handle(RequestReturnCommand command, CancellationToken cancellationToken)
    {
        var order = await dbcontext.Orders.FindAsync([OrderId.Of(command.OrderId)], cancellationToken)
            ?? throw new OrderNotFoundException(command.OrderId);

        // Owner check: a customer can only return their own order.
        if (order.CustomerId != CustomerId.Of(command.CustomerId))
            throw new BadRequestException("You can only return your own orders.");

        try { order.RequestReturn(command.Reason, ReturnWindow); }
        catch (DomainException ex) { throw new BadRequestException(ex.Message); }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return new ReturnResult(true);
    }
}

public class ApproveReturnHandler(IApplicationDbContext dbcontext) : ICommandHandler<ApproveReturnCommand, ReturnResult>
{
    public async Task<ReturnResult> Handle(ApproveReturnCommand command, CancellationToken cancellationToken)
    {
        var order = await dbcontext.Orders.FindAsync([OrderId.Of(command.OrderId)], cancellationToken)
            ?? throw new OrderNotFoundException(command.OrderId);

        try { order.ApproveReturn(); }
        catch (DomainException ex) { throw new BadRequestException(ex.Message); }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return new ReturnResult(true);
    }
}

public class RejectReturnHandler(IApplicationDbContext dbcontext) : ICommandHandler<RejectReturnCommand, ReturnResult>
{
    public async Task<ReturnResult> Handle(RejectReturnCommand command, CancellationToken cancellationToken)
    {
        var order = await dbcontext.Orders.FindAsync([OrderId.Of(command.OrderId)], cancellationToken)
            ?? throw new OrderNotFoundException(command.OrderId);

        try { order.RejectReturn(command.Reason); }
        catch (DomainException ex) { throw new BadRequestException(ex.Message); }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return new ReturnResult(true);
    }
}

public class CompleteRefundHandler(IApplicationDbContext dbcontext) : ICommandHandler<CompleteRefundCommand, ReturnResult>
{
    public async Task<ReturnResult> Handle(CompleteRefundCommand command, CancellationToken cancellationToken)
    {
        var order = await dbcontext.Orders.FindAsync([OrderId.Of(command.OrderId)], cancellationToken)
            ?? throw new OrderNotFoundException(command.OrderId);

        try { order.CompleteRefund(); }
        catch (DomainException ex) { throw new BadRequestException(ex.Message); }

        await dbcontext.SaveChangesAsync(cancellationToken);
        return new ReturnResult(true);
    }
}
