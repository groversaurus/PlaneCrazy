using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlaneCrazy.EventStore;

/// <summary>
/// A JSON file-based event store that saves each event as a separate file
/// under Events/{EntityType}/{EntityId}/ directory structure.
/// </summary>
public class JsonFileEventStore : IEventStore
{
    private readonly string _basePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileEventStore"/> class.
    /// </summary>
    /// <param name="basePath">The base directory path for storing events. Defaults to "Events".</param>
    public JsonFileEventStore(string basePath = "Events")
    {
        _basePath = basePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    /// <summary>
    /// Saves an event to the file system as a JSON file.
    /// Files are saved under Events/{EntityType}/{EntityId}/ with sequential numbering (001.json, 002.json, etc.).
    /// </summary>
    public async Task SaveEventAsync(IEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));
        
        if (string.IsNullOrWhiteSpace(@event.EntityType))
            throw new ArgumentException("EntityType cannot be null or empty.", nameof(@event));
        
        if (string.IsNullOrWhiteSpace(@event.EntityId))
            throw new ArgumentException("EntityId cannot be null or empty.", nameof(@event));

        var entityPath = GetEntityPath(@event.EntityType, @event.EntityId);
        
        // Ensure directory exists
        lock (_lockObject)
        {
            if (!Directory.Exists(entityPath))
            {
                Directory.CreateDirectory(entityPath);
            }
        }

        // Get next sequence number
        var sequenceNumber = GetNextSequenceNumber(entityPath);
        var fileName = $"{sequenceNumber:D3}.json";
        var filePath = Path.Combine(entityPath, fileName);

        // Serialize and save event
        var eventData = new EventData
        {
            EntityId = @event.EntityId,
            EntityType = @event.EntityType,
            EventType = @event.EventType,
            Timestamp = @event.Timestamp,
            Data = @event
        };

        var json = JsonSerializer.Serialize(eventData, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Gets all events for a specific entity.
    /// </summary>
    public async Task<IReadOnlyList<IEvent>> GetEventsAsync(string entityType, string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType cannot be null or empty.", nameof(entityType));
        
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("EntityId cannot be null or empty.", nameof(entityId));

        var entityPath = GetEntityPath(entityType, entityId);
        
        if (!Directory.Exists(entityPath))
            return Array.Empty<IEvent>();

        var events = new List<IEvent>();
        var files = Directory.GetFiles(entityPath, "*.json")
            .OrderBy(f => f)
            .ToList();

        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file);
            var eventData = JsonSerializer.Deserialize<EventData>(json, _jsonOptions);
            
            if (eventData?.Data != null)
            {
                events.Add(eventData.Data);
            }
        }

        return events.AsReadOnly();
    }

    /// <summary>
    /// Gets all events for a specific entity type across all entities of that type.
    /// </summary>
    public async Task<IReadOnlyList<IEvent>> GetEventsByEntityTypeAsync(string entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType cannot be null or empty.", nameof(entityType));

        var typePath = Path.Combine(_basePath, entityType);
        
        if (!Directory.Exists(typePath))
            return Array.Empty<IEvent>();

        var events = new List<IEvent>();
        var entityDirectories = Directory.GetDirectories(typePath);

        foreach (var entityDir in entityDirectories)
        {
            var entityId = Path.GetFileName(entityDir);
            var entityEvents = await GetEventsAsync(entityType, entityId);
            events.AddRange(entityEvents);
        }

        return events.OrderBy(e => e.Timestamp).ToList().AsReadOnly();
    }

    private string GetEntityPath(string entityType, string entityId)
    {
        return Path.Combine(_basePath, entityType, entityId);
    }

    private int GetNextSequenceNumber(string entityPath)
    {
        lock (_lockObject)
        {
            var files = Directory.GetFiles(entityPath, "*.json");
            
            if (files.Length == 0)
                return 1;

            var maxNumber = files
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => !string.IsNullOrEmpty(name) && int.TryParse(name, out _))
                .Select(name => int.Parse(name!))
                .DefaultIfEmpty(0)
                .Max();

            return maxNumber + 1;
        }
    }

    private class EventData
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        
        [JsonConverter(typeof(EventJsonConverter))]
        public IEvent? Data { get; set; }
    }

    private class EventJsonConverter : JsonConverter<IEvent>
    {
        public override IEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var json = doc.RootElement.GetRawText();
            
            // This is a simplified implementation. In a real-world scenario,
            // you would need to register event types and deserialize to the correct type.
            // For now, we'll return a generic event wrapper.
            return JsonSerializer.Deserialize<GenericEvent>(json, options);
        }

        public override void Write(Utf8JsonWriter writer, IEvent value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }

    private class GenericEvent : EventBase
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; set; }
    }
}
