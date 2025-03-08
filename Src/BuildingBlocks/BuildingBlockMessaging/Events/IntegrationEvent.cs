namespace BuildingBlockMessaging.Events;

public record IntegrationEvent
{
    public Guid id => Guid.NewGuid(); 
    public DateTime OccuredOn => DateTime.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName;
}
