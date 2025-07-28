namespace Order.Domain.Abstractions;

/// <summary>
/// Defines an aggregate root in the domain model.
/// Combines entity identity with aggregate behavior.
/// </summary>
/// <typeparam name="T">Type of the aggregate identifier.</typeparam>
public interface IAggregate<T> :IAggregate , IEntity<T>
{

}

/// <summary>
/// Non-generic interface for an aggregate root.
/// Provides access to domain events.
/// </summary>
public interface IAggregate : IEntity
{
    /// <summary>
    /// Collection of domain events recorded by the aggregate.
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Removes and returns all domain events for publishing.
    /// </summary>
    /// <returns>Array of domain events to dispatch.</returns>
    IDomainEvent[] ClearDomainEvents();
}
