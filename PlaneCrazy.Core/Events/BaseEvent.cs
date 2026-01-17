namespace PlaneCrazy.Core.Events;

public abstract class BaseEvent : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string EntityId { get; init; } = string.Empty;
}
