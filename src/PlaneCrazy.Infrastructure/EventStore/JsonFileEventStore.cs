using System.Text.Json;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;

namespace PlaneCrazy.Infrastructure.EventStore;

public class JsonFileEventStore : IEventStore
{
    private readonly string _eventStorePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public JsonFileEventStore()
    {
        _eventStorePath = PlaneCrazyPaths.EventsPath;
    }

    public async Task AppendAsync(DomainEvent domainEvent)
    {
        await _semaphore.WaitAsync();
        try
        {
            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{domainEvent.Id}.json";
            var filePath = Path.Combine(_eventStorePath, fileName);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var eventWrapper = new EventWrapper
            {
                EventType = domainEvent.EventType,
                Data = JsonSerializer.SerializeToElement(domainEvent, domainEvent.GetType(), options)
            };

            var json = JsonSerializer.Serialize(eventWrapper, options);
            await File.WriteAllTextAsync(filePath, json);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IEnumerable<DomainEvent>> GetAllAsync()
    {
        var events = new List<DomainEvent>();
        var files = Directory.GetFiles(_eventStorePath, "*.json").OrderBy(f => f);

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var eventWrapper = JsonSerializer.Deserialize<EventWrapper>(json, options);

                if (eventWrapper?.EventType != null)
                {
                    var domainEvent = DeserializeEvent(eventWrapper.EventType, eventWrapper.Data, options);
                    if (domainEvent != null)
                    {
                        events.Add(domainEvent);
                    }
                }
            }
            catch
            {
                // Skip corrupted files
            }
        }

        return events;
    }

    public async Task<IEnumerable<DomainEvent>> GetByTypeAsync(string eventType)
    {
        var allEvents = await GetAllAsync();
        return allEvents.Where(e => e.EventType == eventType);
    }

    public async Task AppendEventAsync(DomainEvent domainEvent)
    {
        await AppendAsync(domainEvent);
    }

    public async Task<IEnumerable<DomainEvent>> ReadEventsAsync(
        string? streamId = null,
        string? eventType = null,
        DateTime? fromTimestamp = null,
        DateTime? toTimestamp = null)
    {
        var allEvents = await GetAllAsync();
        var filteredEvents = allEvents.AsEnumerable();

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

        // Note: streamId filtering is not currently supported as DomainEvent doesn't have a StreamId property
        // This parameter is reserved for future use when stream support is added

        return filteredEvents;
    }

    public async Task<IEnumerable<DomainEvent>> ReadAllEventsAsync()
    {
        return await GetAllAsync();
    }

    private DomainEvent? DeserializeEvent(string eventType, JsonElement data, JsonSerializerOptions options)
    {
        return eventType switch
        {
            nameof(AircraftFavourited) => data.Deserialize<AircraftFavourited>(options),
            nameof(AircraftUnfavourited) => data.Deserialize<AircraftUnfavourited>(options),
            nameof(TypeFavourited) => data.Deserialize<TypeFavourited>(options),
            nameof(TypeUnfavourited) => data.Deserialize<TypeUnfavourited>(options),
            nameof(AirportFavourited) => data.Deserialize<AirportFavourited>(options),
            nameof(AirportUnfavourited) => data.Deserialize<AirportUnfavourited>(options),
            nameof(CommentAdded) => data.Deserialize<CommentAdded>(options),
            nameof(CommentEdited) => data.Deserialize<CommentEdited>(options),
            nameof(CommentDeleted) => data.Deserialize<CommentDeleted>(options),
            _ => null
        };
    }

    private class EventWrapper
    {
        public required string EventType { get; set; }
        public required JsonElement Data { get; set; }
    }
}
