using MassTransit;
using Microsoft.FeatureManagement;
using Order.Domain.Events;

namespace Order.Application.OrdersCQRS.EventHandlers.Domain;

/// <summary>
/// Handles the domain event when a new order is created.
/// Publishes an integration event if the OrderFullfillment feature is enabled.
/// </summary>
/// <param name="publishEndpoint">Endpoint used to publish integration events to the message bus.</param>
/// <param name="featureManager">Manages feature flags to enable/disable runtime behaviors.</param>
/// <param name="logger">Logger used to log information and diagnostics.</param>
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
