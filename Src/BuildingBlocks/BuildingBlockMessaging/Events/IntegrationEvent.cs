namespace BuildingBlockMessaging.Events;

public record IntegrationEvent
{
    // Assigned once at construction so the event keeps a stable id/timestamp.
    public Guid id { get; init; } = Guid.NewGuid();
    public DateTime OccuredOn { get; init; } = DateTime.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName!;
}
