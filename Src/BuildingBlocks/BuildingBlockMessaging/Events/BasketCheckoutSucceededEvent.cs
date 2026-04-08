namespace BuildingBlockMessaging.Events;

// OUTBOX/SAGA: emitted by Order service when order creation succeeds.
public record BasketCheckoutSucceededEvent : IntegrationEvent
{
    public Guid CheckoutId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public Guid OrderId { get; set; } = default!;
}
