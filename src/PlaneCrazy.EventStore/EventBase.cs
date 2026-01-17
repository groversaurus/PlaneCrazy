namespace PlaneCrazy.EventStore;

/// <summary>
/// Base class for all events.
/// </summary>
public abstract class EventBase : IEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the entity this event belongs to.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the type of the entity this event belongs to.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets the event type name.
    /// </summary>
    public string EventType => GetType().Name;
    
    protected EventBase()
    {
        Timestamp = DateTime.UtcNow;
    }
}
