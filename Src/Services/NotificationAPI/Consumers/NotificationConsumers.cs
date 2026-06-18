namespace NotificationAPI.Consumers;

// Mock notifications: log what would be sent (real email/SMS is out of scope).
// Each service publishes; this service is an independent consumer (event fan-out).

public class OrderStatusNotificationConsumer(ILogger<OrderStatusNotificationConsumer> logger)
    : IConsumer<OrderStatusChangedIntegrationEvent>
{
    public Task Consume(ConsumeContext<OrderStatusChangedIntegrationEvent> context)
    {
        var m = context.Message;
        logger.LogInformation("NOTIFY customer {CustomerId}: order {OrderId} is now {Status}.", m.CustomerId, m.OrderId, m.Status);
        return Task.CompletedTask;
    }
}

public class ReturnRequestedNotificationConsumer(ILogger<ReturnRequestedNotificationConsumer> logger)
    : IConsumer<ReturnRequestedIntegrationEvent>
{
    public Task Consume(ConsumeContext<ReturnRequestedIntegrationEvent> context)
    {
        var m = context.Message;
        logger.LogInformation("NOTIFY customer {CustomerId}: return opened for order {OrderId}.", m.CustomerId, m.OrderId);
        return Task.CompletedTask;
    }
}

public class OrderReturnedNotificationConsumer(ILogger<OrderReturnedNotificationConsumer> logger)
    : IConsumer<OrderReturnedIntegrationEvent>
{
    public Task Consume(ConsumeContext<OrderReturnedIntegrationEvent> context)
    {
        var m = context.Message;
        logger.LogInformation("NOTIFY customer {CustomerId}: order {OrderId} refunded and returned.", m.CustomerId, m.OrderId);
        return Task.CompletedTask;
    }
}
