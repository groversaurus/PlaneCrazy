namespace PlaneCrazy.Domain.Events;

public abstract class DomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    
    public virtual string? GetEntityType() => null;
    public virtual string? GetEntityId() => null;
}
