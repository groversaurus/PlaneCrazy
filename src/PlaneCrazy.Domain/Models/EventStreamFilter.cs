using PlaneCrazy.Domain.Events;

namespace PlaneCrazy.Domain.Models;

/// <summary>
/// Filter criteria for querying event streams.
/// </summary>
public class EventStreamFilter
{
    /// <summary>
    /// Filter by specific event types. Null means all types.
    /// </summary>
    public IEnumerable<string>? EventTypes { get; set; }

    /// <summary>
    /// Filter by aggregate/entity ID. Null means all aggregates.
    /// </summary>
    public string? AggregateId { get; set; }

    /// <summary>
    /// Start of time range (inclusive). Null means no lower bound.
    /// </summary>
    public DateTime? FromTimestamp { get; set; }

    /// <summary>
    /// End of time range (inclusive). Null means no upper bound.
    /// </summary>
    public DateTime? ToTimestamp { get; set; }

    /// <summary>
    /// Search text to filter events. Null means no text filtering.
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of events per page.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Sort order. Default is ascending by timestamp.
    /// </summary>
    public EventStreamSortOrder SortOrder { get; set; } = EventStreamSortOrder.TimestampAscending;
}

/// <summary>
/// Sort order options for event streams.
/// </summary>
public enum EventStreamSortOrder
{
    /// <summary>
    /// Sort by timestamp, oldest first.
    /// </summary>
    TimestampAscending,

    /// <summary>
    /// Sort by timestamp, newest first.
    /// </summary>
    TimestampDescending
}
