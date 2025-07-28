namespace Order.Domain.Abstractions;

/// <summary>
/// Represents a cluster of domain objects that can be treated as a single unit.
/// Maintains its own collection of domain events.
/// </summary>
/// <typeparam name="TId">Type of the aggregate identifier.</typeparam>
public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>
{

    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the list of domain events that have been added to this aggregate.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be dispatched later.
    /// </summary>
    /// <param name="domainEvent">The domain event instance to add.</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);

    }

    /// <summary>
    /// Clears all domain events from the aggregate and returns them.
    /// </summary>
    /// <returns>Array of dequeued domain events.</returns>
    public IDomainEvent[] ClearDomainEvents()
    {
        IDomainEvent[] dequeuedEvents = _domainEvents.ToArray();

        _domainEvents.Clear();

        return dequeuedEvents;
    }
}
