using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;

namespace PlaneCrazy.Tests.Helpers;

/// <summary>
/// In-memory implementation of IEventStore for testing.
/// Provides fast, isolated event store without file system dependencies.
/// </summary>
public class InMemoryEventStore : IEventStore
{
    private readonly List<DomainEvent> _events = new();
    private readonly object _lock = new();

    public Task AppendAsync(DomainEvent domainEvent)
    {
        lock (_lock)
        {
            _events.Add(domainEvent);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<DomainEvent>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<DomainEvent>>(_events.OrderBy(e => e.OccurredAt).ToList());
        }
    }

    public Task<IEnumerable<DomainEvent>> GetByTypeAsync(string eventType)
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<DomainEvent>>(
                _events.Where(e => e.EventType == eventType)
                       .OrderBy(e => e.OccurredAt)
                       .ToList());
        }
    }

    public Task AppendEventAsync(DomainEvent domainEvent)
    {
        return AppendAsync(domainEvent);
    }

    public Task<IEnumerable<DomainEvent>> ReadEventsAsync(
        string? streamId = null,
        string? eventType = null,
        DateTime? fromTimestamp = null,
        DateTime? toTimestamp = null)
    {
        lock (_lock)
        {
            var filteredEvents = _events.AsEnumerable();

            if (!string.IsNullOrEmpty(eventType))
            {
                filteredEvents = filteredEvents.Where(e => e.EventType == eventType);
            }

            if (fromTimestamp.HasValue)
            {
                filteredEvents = filteredEvents.Where(e => e.OccurredAt >= fromTimestamp.Value);
            }

            if (toTimestamp.HasValue)
            {
                filteredEvents = filteredEvents.Where(e => e.OccurredAt <= toTimestamp.Value);
            }

            return Task.FromResult<IEnumerable<DomainEvent>>(
                filteredEvents.OrderBy(e => e.OccurredAt).ToList());
        }
    }

    public Task<IEnumerable<DomainEvent>> ReadAllEventsAsync()
    {
        return GetAllAsync();
    }

    public void Clear()
    {
        lock (_lock)
        {
            _events.Clear();
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _events.Count;
            }
        }
    }
}
