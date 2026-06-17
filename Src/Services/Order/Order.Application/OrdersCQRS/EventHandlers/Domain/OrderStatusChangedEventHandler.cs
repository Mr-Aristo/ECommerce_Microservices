using BuildingBlockMessaging.Events;
using MassTransit;
using Order.Domain.Events;

namespace Order.Application.OrdersCQRS.EventHandlers.Domain;

/// <summary>
/// Publishes the OrderStatusChanged integration event whenever an order transitions status.
/// </summary>
public class OrderStatusChangedEventHandler(IPublishEndpoint publishEndpoint, ILogger<OrderStatusChangedEventHandler> logger)
    : INotificationHandler<OrderStatusChangedEvent>
{
    public async Task Handle(OrderStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        var order = domainEvent.order;
        logger.LogInformation("Order {OrderId} status changed to {Status}", order.Id.Value, order.Status);

        await publishEndpoint.Publish(new OrderStatusChangedIntegrationEvent
        {
            OrderId = order.Id.Value,
            CustomerId = order.CustomerId.Value,
            Status = order.Status.ToString()
        }, cancellationToken);
    }
}
