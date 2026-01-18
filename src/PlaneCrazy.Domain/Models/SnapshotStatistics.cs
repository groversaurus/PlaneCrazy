namespace PlaneCrazy.Domain.Models;

/// <summary>
/// Statistics about aircraft activity over a time range.
/// </summary>
public class SnapshotStatistics
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public int TotalEvents { get; init; }
    public int UniqueAircraft { get; init; }
    public int FirstSeenCount { get; init; }
    public int PositionUpdateCount { get; init; }
    public int IdentityUpdateCount { get; init; }
    public int LastSeenCount { get; init; }

    public TimeSpan Duration => EndTime - StartTime;
    
    public double EventsPerMinute =>
        Duration.TotalMinutes > 0
            ? TotalEvents / Duration.TotalMinutes
            : 0;

    public double AverageEventsPerAircraft =>
        UniqueAircraft > 0
            ? (double)TotalEvents / UniqueAircraft
            : 0;
}
