namespace PlaneCrazy.Core.Events;

public interface IEvent
{
    Guid EventId { get; }
    DateTime Timestamp { get; }
    string EntityId { get; }
}
