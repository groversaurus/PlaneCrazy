namespace PlaneCrazy.EventStore.Events;

/// <summary>
/// Event raised when an aircraft's squawk code changes.
/// </summary>
public class SquawkChangedEvent : EventBase
{
    /// <summary>
    /// Gets or sets the previous squawk code.
    /// </summary>
    public string? PreviousSquawk { get; set; }
    
    /// <summary>
    /// Gets or sets the new squawk code.
    /// </summary>
    public string NewSquawk { get; set; } = string.Empty;
}
