namespace Order.Domain.Abstractions;

/// <summary>
/// Represents a domain event in the system.
/// Inherits from MediatR INotification for in-process dispatch.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId => Guid.CreateVersion7(); // .net 9 guid (faster)

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime OccuredOn => DateTime.Now;

    /// <summary>
    /// Full assembly-qualified name of the event type.
    /// </summary>
    public string? EventType => GetType().AssemblyQualifiedName;
}

/*
    Domain Events represent something that happened in the
    past and the other parts of the same service boundart
    same domain need to react to these changes.
    Domain Event is a business event that occurs within the domain model.
    It often represents a side effect of a domain operation.
    Achieve consistency between aggreagates in the same domain. 
*/