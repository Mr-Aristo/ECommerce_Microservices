using Order.Domain.Events;

namespace Order.Application.OrdersCQRS.EventHandlers.Domain;

public class OrderUpdateEventHandler(ILogger<OrderUpdateEventHandler> logger) : INotificationHandler<OrderUpdatedEvent>
{
    public Task Handle(OrderUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event handeled: {DomainEvent}", notification.GetType().Name);
        return Task.CompletedTask;  
    }
}
