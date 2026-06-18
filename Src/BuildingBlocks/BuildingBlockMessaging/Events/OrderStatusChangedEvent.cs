namespace BuildingBlockMessaging.Events;

// Published by Order when an order's status changes; consumers (notifications, read models) react.
public record OrderStatusChangedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; } = default!;
    public Guid CustomerId { get; set; } = default!;
    public string Status { get; set; } = default!;
}
