namespace PlaneCrazy.Domain.Models;

/// <summary>
/// Statistics about the event stream.
/// </summary>
public class EventStreamStatistics
{
    /// <summary>
    /// Total number of events in the stream.
    /// </summary>
    public int TotalEventCount { get; set; }

    /// <summary>
    /// Breakdown of event counts by type.
    /// </summary>
    public Dictionary<string, int> EventCountsByType { get; set; } = new();

    /// <summary>
    /// Timestamp of the oldest event in the stream.
    /// </summary>
    public DateTime? OldestEventTimestamp { get; set; }

    /// <summary>
    /// Timestamp of the newest event in the stream.
    /// </summary>
    public DateTime? NewestEventTimestamp { get; set; }

    /// <summary>
    /// Total size of event store in bytes.
    /// </summary>
    public long TotalStorageBytes { get; set; }

    /// <summary>
    /// Average events per day.
    /// </summary>
    public double AverageEventsPerDay { get; set; }

    /// <summary>
    /// Most common event type.
    /// </summary>
    public string? MostCommonEventType { get; set; }

    /// <summary>
    /// Number of unique aggregate IDs.
    /// </summary>
    public int UniqueAggregateCount { get; set; }
}
