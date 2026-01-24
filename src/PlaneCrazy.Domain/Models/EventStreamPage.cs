using PlaneCrazy.Domain.Events;

namespace PlaneCrazy.Domain.Models;

/// <summary>
/// Represents a paginated result of events from the event stream.
/// </summary>
public class EventStreamPage
{
    /// <summary>
    /// The events in this page.
    /// </summary>
    public IEnumerable<DomainEvent> Events { get; set; } = Enumerable.Empty<DomainEvent>();

    /// <summary>
    /// Total number of events matching the query (across all pages).
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of events per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// The timestamp of the first event in this page (if any).
    /// </summary>
    public DateTime? FirstEventTimestamp => Events.FirstOrDefault()?.OccurredAt;

    /// <summary>
    /// The timestamp of the last event in this page (if any).
    /// </summary>
    public DateTime? LastEventTimestamp => Events.LastOrDefault()?.OccurredAt;
}
