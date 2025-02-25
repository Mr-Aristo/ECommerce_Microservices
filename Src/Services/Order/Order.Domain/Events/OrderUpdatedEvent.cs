namespace Order.Domain.Events;
/// <summary>
/// Domain Event Pattern
/// Catches events
/// </summary>
public record OrderUpdatedEvent(Orders order) : IDomainEvent;

