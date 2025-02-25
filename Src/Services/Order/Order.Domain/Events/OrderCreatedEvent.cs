namespace Order.Domain.Events;
/// <summary>
/// Domain Event Pattern
/// Catches events
/// </summary>
/// <param name="order"></param>
public record OrderCreatedEvent(Orders order):IDomainEvent;




