namespace Order.Domain.Events;

/// <summary>
/// Raised when a new Order is created in the system.
/// Used in the Domain Event Pattern to notify the application layer
/// or trigger side effects without directly coupling to other services.
/// </summary>
/// <param name="order">The newly created order instance.</param>
public record OrderCreatedEvent(Orders order) : IDomainEvent;




