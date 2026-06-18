namespace Order.Domain.Events;

/// <summary>
/// Raised when an order's status transitions (e.g. Confirmed -> Processing -> Shipped).
/// Handled to publish the OrderStatusChanged integration event.
/// </summary>
public record OrderStatusChangedEvent(Orders order) : IDomainEvent;
