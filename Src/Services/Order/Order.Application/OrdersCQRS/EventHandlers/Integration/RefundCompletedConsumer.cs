using BuildingBlockMessaging.Events;
using MassTransit;
using Order.Application.OrdersCQRS.Commands.Returns;

namespace Order.Application.OrdersCQRS.EventHandlers.Integration;

/// <summary>
/// Consumes RefundCompleted from the Payment service and finalizes the return (Order -> Returned).
/// </summary>
public class RefundCompletedConsumer(ISender sender, ILogger<RefundCompletedConsumer> logger)
    : IConsumer<RefundCompletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<RefundCompletedIntegrationEvent> context)
    {
        logger.LogInformation("Refund completed for order {OrderId}; finalizing return.", context.Message.OrderId);
        await sender.Send(new CompleteRefundCommand(context.Message.OrderId), context.CancellationToken);
    }
}
