using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Models;

namespace PlaneCrazy.Domain.Interfaces;

/// <summary>
/// Service for querying, filtering, and paginating event streams.
/// Provides rich querying capabilities for debugging, admin tools, and UI consumption.
/// </summary>
public interface IEventStreamService
{
    /// <summary>
    /// Retrieves a paginated list of events with optional filtering.
    /// </summary>
    /// <param name="filter">Filter criteria for events.</param>
    /// <returns>A paginated result containing events and metadata.</returns>
    Task<EventStreamPage> GetEventsAsync(EventStreamFilter filter);

    /// <summary>
    /// Retrieves events for a specific aggregate/entity by ID.
    /// </summary>
    /// <param name="aggregateId">The aggregate/entity identifier.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of events per page.</param>
    /// <returns>A paginated result containing events for the aggregate.</returns>
    Task<EventStreamPage> GetEventsByAggregateAsync(string aggregateId, int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Retrieves events within a specific time range.
    /// </summary>
    /// <param name="fromTimestamp">Start of time range (inclusive).</param>
    /// <param name="toTimestamp">End of time range (inclusive).</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of events per page.</param>
    /// <returns>A paginated result containing events within the time range.</returns>
    Task<EventStreamPage> GetEventsByTimeRangeAsync(
        DateTime fromTimestamp, 
        DateTime toTimestamp, 
        int pageNumber = 1, 
        int pageSize = 50);

    /// <summary>
    /// Retrieves events of a specific type.
    /// </summary>
    /// <param name="eventType">The event type to filter by.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of events per page.</param>
    /// <returns>A paginated result containing events of the specified type.</returns>
    Task<EventStreamPage> GetEventsByTypeAsync(string eventType, int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Gets statistics about the event stream.
    /// </summary>
    /// <returns>Statistics about event counts, types, and time ranges.</returns>
    Task<EventStreamStatistics> GetStatisticsAsync();

    /// <summary>
    /// Gets a breakdown of event counts by type.
    /// </summary>
    /// <returns>A dictionary mapping event type names to their counts.</returns>
    Task<Dictionary<string, int>> GetEventTypeBreakdownAsync();

    /// <summary>
    /// Searches for events containing specific text in their serialized JSON representation.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of events per page.</param>
    /// <returns>A paginated result containing matching events.</returns>
    Task<EventStreamPage> SearchEventsAsync(string searchText, int pageNumber = 1, int pageSize = 50);

    /// <summary>
    /// Exports events to JSON for backup or analysis.
    /// </summary>
    /// <param name="filter">Filter criteria for events to export.</param>
    /// <returns>A JSON string containing the filtered events.</returns>
    Task<string> ExportEventsAsync(EventStreamFilter filter);
}