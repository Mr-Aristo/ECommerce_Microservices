namespace BuildingBlockMessaging.Events;

// Published by the Payment service once a refund is processed; Order finalizes the return (-> Returned).
public record RefundCompletedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; } = default!;
    public Guid CustomerId { get; set; } = default!;
    public decimal Amount { get; set; } = default!;
}
