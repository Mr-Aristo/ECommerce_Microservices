using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Order.Domain.Events;

namespace Order.Application.OrdersCQRS.EventHandlers.Domain;

public class OrderCreateEventHandler(IPublishEndpoint publishEndpoint, IFeatureManager featureManager, ILogger<OrderCreateEventHandler> logger) : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Evenet handled : {DomainEvent}",domainEvent.GetType().Name);

        if (await featureManager.IsEnabledAsync("OrderFullfillment"))
        {
            var orderCreateIntegrationEvent = domainEvent.order.ToOrderDto();
            await publishEndpoint.Publish(orderCreateIntegrationEvent, cancellationToken);
        }
    }
}
