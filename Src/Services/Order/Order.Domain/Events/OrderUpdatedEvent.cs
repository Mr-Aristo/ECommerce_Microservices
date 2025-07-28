namespace Order.Domain.Events;

/// <summary>
/// Raised when an existing Order is updated.
/// Part of the Domain Event Pattern, allowing decoupled reactions
/// to changes in the domain model.
/// </summary>
/// <param name="order">The updated order instance.</param>
public record OrderUpdatedEvent(Orders order) : IDomainEvent;

