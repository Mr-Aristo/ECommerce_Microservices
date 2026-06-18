namespace BuildingBlockMessaging.Events;

// Published by Order when a customer opens a return.
public record ReturnRequestedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; } = default!;
    public Guid CustomerId { get; set; } = default!;
    public string Reason { get; set; } = default!;
}

// Published by Order when support approves a return; the Payment service refunds against this.
public record ReturnApprovedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; } = default!;
    public Guid CustomerId { get; set; } = default!;
    public decimal Amount { get; set; } = default!;
}

// Published by Order when the refund completes and the order is marked Returned.
public record OrderReturnedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; } = default!;
    public Guid CustomerId { get; set; } = default!;
}
