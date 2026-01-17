namespace PlaneCrazy.Core.Events;

public abstract class BaseEvent : IEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EntityId { get; set; } = string.Empty;
}
