namespace BuildingBlockMessaging.Events;

// OUTBOX/SAGA: emitted by Order service when checkout processing fails.
public record BasketCheckoutFailedEvent : IntegrationEvent
{
    public Guid CheckoutId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Reason { get; set; } = default!;
}
