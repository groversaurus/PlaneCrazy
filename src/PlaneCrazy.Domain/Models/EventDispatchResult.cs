namespace PlaneCrazy.Domain.Models;

/// <summary>
/// Result of dispatching a single event.
/// </summary>
public class EventDispatchResult
{
    public Guid EventId { get; init; }
    public required string EventType { get; init; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Exception? Exception { get; set; }
    public long EventStoreWriteTimeMs { get; set; }
    public long TotalTimeMs { get; set; }
    public IEnumerable<ProjectionUpdateResult> ProjectionResults { get; set; } = Enumerable.Empty<ProjectionUpdateResult>();
    
    public int ProjectionsUpdated => ProjectionResults.Count(r => r.Success && r.EventHandled);
    public int ProjectionsFailed => ProjectionResults.Count(r => !r.Success);
}

/// <summary>
/// Result of updating a single projection.
/// </summary>
public class ProjectionUpdateResult
{
    public required string ProjectionName { get; init; }
    public bool Success { get; set; }
    public bool EventHandled { get; set; }
    public string? Error { get; set; }
    public Exception? Exception { get; set; }
    public long UpdateTimeMs { get; set; }
}

/// <summary>
/// Result of dispatching multiple events.
/// </summary>
public class BatchDispatchResult
{
    public int TotalEvents { get; init; }
    public int SuccessfulEvents { get; init; }
    public int FailedEvents { get; init; }
    public IEnumerable<EventDispatchResult> EventResults { get; init; } = Enumerable.Empty<EventDispatchResult>();
    public long TotalTimeMs { get; init; }
    
    public bool AllSuccessful => FailedEvents == 0;
    public double SuccessRate => TotalEvents > 0 ? (double)SuccessfulEvents / TotalEvents : 0;
}

/// <summary>
/// Statistics about registered projections.
/// </summary>
public class ProjectionStatistics
{
    public int TotalProjections { get; init; }
    public required List<string> ProjectionNames { get; init; }
}
