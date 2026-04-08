using BuildingBlockMessaging.Events;

namespace BasketAPI.Models;

// OUTBOX/SAGA: persisted message envelope used by Basket local outbox.
public class BasketCheckoutOutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CheckoutId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public BasketCheckoutEvent Payload { get; set; } = default!;
    public CheckoutOutboxStatus Status { get; set; } = CheckoutOutboxStatus.Pending;
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}
