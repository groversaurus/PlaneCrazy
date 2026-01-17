using System.Text.Json;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;

namespace PlaneCrazy.Infrastructure.EventStore;

public class JsonFileEventStore : IEventStore
{
    private readonly string _eventStorePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public JsonFileEventStore(string basePath)
    {
        _eventStorePath = Path.Combine(basePath, "EventStore");
        Directory.CreateDirectory(_eventStorePath);
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
                Data = domainEvent
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
                var eventWrapper = JsonSerializer.Deserialize<EventWrapper>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (eventWrapper?.Data != null)
                {
                    events.Add(eventWrapper.Data);
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

    private class EventWrapper
    {
        public required string EventType { get; set; }
        public required DomainEvent Data { get; set; }
    }
}
