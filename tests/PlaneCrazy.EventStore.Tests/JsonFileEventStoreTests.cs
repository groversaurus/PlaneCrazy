using PlaneCrazy.EventStore;
using PlaneCrazy.EventStore.Events;
using Xunit;

namespace PlaneCrazy.EventStore.Tests;

public class JsonFileEventStoreTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly JsonFileEventStore _eventStore;

    public JsonFileEventStoreTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"EventStoreTests_{Guid.NewGuid()}");
        _eventStore = new JsonFileEventStore(_testBasePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, recursive: true);
        }
    }

    [Fact]
    public async Task SaveEventAsync_CreatesDirectoryStructure()
    {
        // Arrange
        var @event = new AircraftDetectedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            IcaoAddress = "ABC123",
            Callsign = "UAL123"
        };

        // Act
        await _eventStore.SaveEventAsync(@event);

        // Assert
        var expectedPath = Path.Combine(_testBasePath, "Aircraft", "ABC123");
        Assert.True(Directory.Exists(expectedPath));
    }

    [Fact]
    public async Task SaveEventAsync_CreatesJsonFile()
    {
        // Arrange
        var @event = new AircraftDetectedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            IcaoAddress = "ABC123",
            Callsign = "UAL123"
        };

        // Act
        await _eventStore.SaveEventAsync(@event);

        // Assert
        var expectedFile = Path.Combine(_testBasePath, "Aircraft", "ABC123", "001.json");
        Assert.True(File.Exists(expectedFile));
    }

    [Fact]
    public async Task SaveEventAsync_UsesSequentialNumbering()
    {
        // Arrange
        var event1 = new AircraftDetectedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            IcaoAddress = "ABC123"
        };

        var event2 = new PositionUpdatedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Altitude = 35000
        };

        var event3 = new SquawkChangedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            NewSquawk = "7700"
        };

        // Act
        await _eventStore.SaveEventAsync(event1);
        await _eventStore.SaveEventAsync(event2);
        await _eventStore.SaveEventAsync(event3);

        // Assert
        var entityPath = Path.Combine(_testBasePath, "Aircraft", "ABC123");
        Assert.True(File.Exists(Path.Combine(entityPath, "001.json")));
        Assert.True(File.Exists(Path.Combine(entityPath, "002.json")));
        Assert.True(File.Exists(Path.Combine(entityPath, "003.json")));
    }

    [Fact]
    public async Task SaveEventAsync_ThrowsArgumentNullException_WhenEventIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _eventStore.SaveEventAsync(null!));
    }

    [Fact]
    public async Task SaveEventAsync_ThrowsArgumentException_WhenEntityTypeIsEmpty()
    {
        // Arrange
        var @event = new AircraftDetectedEvent
        {
            EntityType = "",
            EntityId = "ABC123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _eventStore.SaveEventAsync(@event));
    }

    [Fact]
    public async Task SaveEventAsync_ThrowsArgumentException_WhenEntityIdIsEmpty()
    {
        // Arrange
        var @event = new AircraftDetectedEvent
        {
            EntityType = "Aircraft",
            EntityId = ""
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _eventStore.SaveEventAsync(@event));
    }

    [Fact]
    public async Task GetEventsAsync_ReturnsEmptyList_WhenNoEventsExist()
    {
        // Act
        var events = await _eventStore.GetEventsAsync("Aircraft", "XYZ789");

        // Assert
        Assert.Empty(events);
    }

    [Fact]
    public async Task GetEventsAsync_ReturnsAllEventsForEntity()
    {
        // Arrange
        var event1 = new AircraftDetectedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            IcaoAddress = "ABC123",
            Callsign = "UAL123"
        };

        var event2 = new PositionUpdatedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Altitude = 35000
        };

        await _eventStore.SaveEventAsync(event1);
        await _eventStore.SaveEventAsync(event2);

        // Act
        var events = await _eventStore.GetEventsAsync("Aircraft", "ABC123");

        // Assert
        Assert.Equal(2, events.Count);
        Assert.Equal("ABC123", events[0].EntityId);
        Assert.Equal("ABC123", events[1].EntityId);
    }

    [Fact]
    public async Task GetEventsAsync_ReturnsEventsInOrder()
    {
        // Arrange
        var event1 = new AircraftDetectedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            IcaoAddress = "ABC123"
        };

        await Task.Delay(10); // Ensure different timestamps

        var event2 = new PositionUpdatedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Altitude = 35000
        };

        await _eventStore.SaveEventAsync(event1);
        await _eventStore.SaveEventAsync(event2);

        // Act
        var events = await _eventStore.GetEventsAsync("Aircraft", "ABC123");

        // Assert
        Assert.Equal(2, events.Count);
        Assert.True(events[0].Timestamp <= events[1].Timestamp);
    }

    [Fact]
    public async Task GetEventsByEntityTypeAsync_ReturnsEmptyList_WhenNoEventsExist()
    {
        // Act
        var events = await _eventStore.GetEventsByEntityTypeAsync("Aircraft");

        // Assert
        Assert.Empty(events);
    }

    [Fact]
    public async Task GetEventsByEntityTypeAsync_ReturnsAllEventsForEntityType()
    {
        // Arrange
        var event1 = new AircraftDetectedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            IcaoAddress = "ABC123"
        };

        var event2 = new AircraftDetectedEvent
        {
            EntityType = "Aircraft",
            EntityId = "XYZ789",
            IcaoAddress = "XYZ789"
        };

        var event3 = new PositionUpdatedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Altitude = 35000
        };

        await _eventStore.SaveEventAsync(event1);
        await _eventStore.SaveEventAsync(event2);
        await _eventStore.SaveEventAsync(event3);

        // Act
        var events = await _eventStore.GetEventsByEntityTypeAsync("Aircraft");

        // Assert
        Assert.Equal(3, events.Count);
    }

    [Fact]
    public async Task GetEventsByEntityTypeAsync_ThrowsArgumentException_WhenEntityTypeIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _eventStore.GetEventsByEntityTypeAsync(""));
    }

    [Fact]
    public async Task GetEventsAsync_ThrowsArgumentException_WhenEntityTypeIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _eventStore.GetEventsAsync("", "ABC123"));
    }

    [Fact]
    public async Task GetEventsAsync_ThrowsArgumentException_WhenEntityIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _eventStore.GetEventsAsync("Aircraft", ""));
    }

    [Fact]
    public async Task MultipleEntitiesOfSameType_AreStoredSeparately()
    {
        // Arrange
        var event1 = new AircraftDetectedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            IcaoAddress = "ABC123"
        };

        var event2 = new AircraftDetectedEvent
        {
            EntityType = "Aircraft",
            EntityId = "XYZ789",
            IcaoAddress = "XYZ789"
        };

        await _eventStore.SaveEventAsync(event1);
        await _eventStore.SaveEventAsync(event2);

        // Act
        var events1 = await _eventStore.GetEventsAsync("Aircraft", "ABC123");
        var events2 = await _eventStore.GetEventsAsync("Aircraft", "XYZ789");

        // Assert
        Assert.Single(events1);
        Assert.Single(events2);
        Assert.Equal("ABC123", events1[0].EntityId);
        Assert.Equal("XYZ789", events2[0].EntityId);
    }

    [Fact]
    public async Task JsonFiles_ContainEventData()
    {
        // Arrange
        var @event = new AircraftDetectedEvent
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            IcaoAddress = "ABC123",
            Callsign = "UAL123",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Altitude = 35000
        };

        await _eventStore.SaveEventAsync(@event);

        // Act
        var filePath = Path.Combine(_testBasePath, "Aircraft", "ABC123", "001.json");
        var jsonContent = await File.ReadAllTextAsync(filePath);

        // Assert
        Assert.Contains("ABC123", jsonContent);
        Assert.Contains("UAL123", jsonContent);
        Assert.Contains("Aircraft", jsonContent);
    }
}
