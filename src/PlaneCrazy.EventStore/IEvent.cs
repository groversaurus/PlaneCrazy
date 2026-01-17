namespace PlaneCrazy.EventStore;

/// <summary>
/// Represents an event in the event store.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Gets the unique identifier of the entity this event belongs to.
    /// </summary>
    string EntityId { get; }
    
    /// <summary>
    /// Gets the type of the entity this event belongs to.
    /// </summary>
    string EntityType { get; }
    
    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    DateTime Timestamp { get; }
    
    /// <summary>
    /// Gets the event type name.
    /// </summary>
    string EventType { get; }
}
