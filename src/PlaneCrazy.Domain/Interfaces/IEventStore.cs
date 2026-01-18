using PlaneCrazy.Domain.Events;

namespace PlaneCrazy.Domain.Interfaces;

public interface IEventStore
{
    Task AppendAsync(DomainEvent domainEvent);
    Task<IEnumerable<DomainEvent>> GetAllAsync();
    Task<IEnumerable<DomainEvent>> GetByTypeAsync(string eventType);

    /// <summary>
    /// Appends a domain event to the event store.
    /// Alias for AppendAsync with more explicit naming.
    /// </summary>
    /// <param name="domainEvent">The domain event to append.</param>
    Task AppendEventAsync(DomainEvent domainEvent);

    /// <summary>
    /// Reads events from the event store with optional filtering.
    /// </summary>
    /// <param name="streamId">Optional stream/aggregate identifier to filter events.</param>
    /// <param name="eventType">Optional event type to filter by.</param>
    /// <param name="fromTimestamp">Optional start timestamp to filter events from.</param>
    /// <param name="toTimestamp">Optional end timestamp to filter events to.</param>
    /// <returns>A collection of domain events matching the filter criteria.</returns>
    Task<IEnumerable<DomainEvent>> ReadEventsAsync(
        string? streamId = null,
        string? eventType = null,
        DateTime? fromTimestamp = null,
        DateTime? toTimestamp = null);

    /// <summary>
    /// Reads all events from the event store.
    /// Alias for GetAllAsync with more explicit naming.
    /// </summary>
    /// <returns>All domain events in the store.</returns>
    Task<IEnumerable<DomainEvent>> ReadAllEventsAsync();
}
