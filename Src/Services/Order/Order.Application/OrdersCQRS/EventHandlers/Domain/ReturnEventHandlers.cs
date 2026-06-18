using BuildingBlockMessaging.Events;
using MassTransit;
using Order.Domain.Events;

namespace Order.Application.OrdersCQRS.EventHandlers.Domain;

public class ReturnRequestedEventHandler(IPublishEndpoint publishEndpoint, ILogger<ReturnRequestedEventHandler> logger)
    : INotificationHandler<ReturnRequestedEvent>
{
    public async Task Handle(ReturnRequestedEvent domainEvent, CancellationToken cancellationToken)
    {
        var order = domainEvent.order;
        logger.LogInformation("Return requested for order {OrderId}", order.Id.Value);
        await publishEndpoint.Publish(new ReturnRequestedIntegrationEvent
        {
            OrderId = order.Id.Value,
            CustomerId = order.CustomerId.Value,
            Reason = order.Return!.Reason
        }, cancellationToken);
    }
}

public class ReturnApprovedEventHandler(IPublishEndpoint publishEndpoint, ILogger<ReturnApprovedEventHandler> logger)
    : INotificationHandler<ReturnApprovedEvent>
{
    public async Task Handle(ReturnApprovedEvent domainEvent, CancellationToken cancellationToken)
    {
        var order = domainEvent.order;
        logger.LogInformation("Return approved for order {OrderId}", order.Id.Value);
        await publishEndpoint.Publish(new ReturnApprovedIntegrationEvent
        {
            OrderId = order.Id.Value,
            CustomerId = order.CustomerId.Value,
            Amount = order.TotalPrice
        }, cancellationToken);
    }
}

public class OrderReturnedEventHandler(IPublishEndpoint publishEndpoint, ILogger<OrderReturnedEventHandler> logger)
    : INotificationHandler<OrderReturnedEvent>
{
    public async Task Handle(OrderReturnedEvent domainEvent, CancellationToken cancellationToken)
    {
        var order = domainEvent.order;
        logger.LogInformation("Order {OrderId} returned", order.Id.Value);
        await publishEndpoint.Publish(new OrderReturnedIntegrationEvent
        {
            OrderId = order.Id.Value,
            CustomerId = order.CustomerId.Value
        }, cancellationToken);
    }
}
