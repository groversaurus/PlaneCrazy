using PlaneCrazy.Domain.Events;
using PlaneCrazy.Infrastructure.EventStore;
using PlaneCrazy.Tests.Helpers;

namespace PlaneCrazy.Tests.EventStore;

/// <summary>
/// Tests for JsonFileEventStore implementation.
/// Uses temporary directories to avoid file system dependencies.
/// </summary>
public class JsonFileEventStoreTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _originalEventsPath;

    public JsonFileEventStoreTests()
    {
        // Create a temporary test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), $"PlaneCrazyTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        // Store original path and set test path
        _originalEventsPath = PlaneCrazyPaths.EventsPath;
        
        // We need to create the events directory for the test
        var eventsPath = Path.Combine(_testDirectory, "Events");
        Directory.CreateDirectory(eventsPath);
        
        // Note: PlaneCrazyPaths is static, so we use the default location
        // For true isolation, we'd need to refactor JsonFileEventStore to accept a path
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task AppendAsync_ValidEvent_WritesEventToFile()
    {
        // Arrange
        var eventStore = new JsonFileEventStore();
        var @event = TestHelpers.CreateCommentAddedEvent();
        
        // Get the expected file count before
        var filesBefore = Directory.GetFiles(PlaneCrazyPaths.EventsPath, "*.json").Length;

        // Act
        await eventStore.AppendAsync(@event);

        // Assert - verify file was created
        var filesAfter = Directory.GetFiles(PlaneCrazyPaths.EventsPath, "*.json").Length;
        Assert.Equal(filesBefore + 1, filesAfter);
        
        // Verify we can read it back
        var allEvents = await eventStore.GetAllAsync();
        var readEvent = allEvents.FirstOrDefault(e => e.Id == @event.Id);
        Assert.NotNull(readEvent);
        Assert.Equal(@event.EventType, readEvent.EventType);
    }

    [Fact]
    public async Task AppendAsync_MultipleEvents_MaintainsOrder()
    {
        // Arrange
        var eventStore = new JsonFileEventStore();
        var events = new List<CommentAdded>();
        
        // Create events with small delays to ensure different timestamps
        for (int i = 0; i < 3; i++)
        {
            await Task.Delay(10); // Ensure distinct timestamps
            events.Add(TestHelpers.CreateCommentAddedEvent(
                text: $"Comment {i}",
                timestamp: DateTime.UtcNow));
        }

        // Act
        foreach (var @event in events)
        {
            await eventStore.AppendAsync(@event);
        }

        // Assert
        var allEvents = (await eventStore.GetAllAsync()).ToList();
        var ourEvents = allEvents.Where(e => events.Any(ev => ev.Id == e.Id))
                                 .OrderBy(e => e.OccurredAt)
                                 .ToList();
        
        Assert.Equal(3, ourEvents.Count);
        
        // Verify chronological order
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(events[i].Id, ourEvents[i].Id);
        }
    }

    [Fact]
    public async Task GetAllAsync_WithEvents_ReturnsAllEventsInOrder()
    {
        // Arrange
        var eventStore = new JsonFileEventStore();
        var commentEvent = TestHelpers.CreateCommentAddedEvent();
        await Task.Delay(10);
        var favouriteEvent = TestHelpers.CreateAircraftFavouritedEvent();
        
        await eventStore.AppendAsync(commentEvent);
        await eventStore.AppendAsync(favouriteEvent);

        // Act
        var allEvents = await eventStore.GetAllAsync();

        // Assert
        var eventsList = allEvents.ToList();
        Assert.Contains(eventsList, e => e.Id == commentEvent.Id);
        Assert.Contains(eventsList, e => e.Id == favouriteEvent.Id);
        
        // Verify chronological order
        var orderedEvents = eventsList.OrderBy(e => e.OccurredAt).ToList();
        Assert.Equal(eventsList, orderedEvents);
    }

    [Fact]
    public async Task GetByTypeAsync_FiltersByEventType()
    {
        // Arrange
        var eventStore = new JsonFileEventStore();
        
        var commentAdded = TestHelpers.CreateCommentAddedEvent();
        var commentId = commentAdded.CommentId;
        
        await Task.Delay(10);
        var commentEdited = TestHelpers.CreateCommentEditedEvent(commentId);
        
        await Task.Delay(10);
        var aircraftFavourited = TestHelpers.CreateAircraftFavouritedEvent();

        await eventStore.AppendAsync(commentAdded);
        await eventStore.AppendAsync(commentEdited);
        await eventStore.AppendAsync(aircraftFavourited);

        // Act
        var commentAddedEvents = await eventStore.GetByTypeAsync(nameof(CommentAdded));
        var commentEditedEvents = await eventStore.GetByTypeAsync(nameof(CommentEdited));
        var aircraftFavouritedEvents = await eventStore.GetByTypeAsync(nameof(AircraftFavourited));

        // Assert
        Assert.Contains(commentAddedEvents, e => e.Id == commentAdded.Id);
        Assert.Contains(commentEditedEvents, e => e.Id == commentEdited.Id);
        Assert.Contains(aircraftFavouritedEvents, e => e.Id == aircraftFavourited.Id);
        
        Assert.DoesNotContain(commentAddedEvents, e => e.Id == commentEdited.Id);
        Assert.DoesNotContain(commentEditedEvents, e => e.Id == aircraftFavourited.Id);
    }

    [Fact]
    public async Task ReadEventsAsync_FiltersCorrectly()
    {
        // Arrange
        var eventStore = new JsonFileEventStore();
        
        var baseTime = DateTime.UtcNow;
        var event1 = TestHelpers.CreateCommentAddedEvent(text: "Event 1", timestamp: baseTime.AddMinutes(-10));
        await Task.Delay(10);
        var event2 = TestHelpers.CreateCommentEditedEvent(event1.CommentId, text: "Event 2", timestamp: baseTime.AddMinutes(-5));
        await Task.Delay(10);
        var event3 = TestHelpers.CreateAircraftFavouritedEvent();

        await eventStore.AppendAsync(event1);
        await eventStore.AppendAsync(event2);
        await eventStore.AppendAsync(event3);

        // Act & Assert - Filter by event type
        var commentEvents = await eventStore.ReadEventsAsync(eventType: nameof(CommentAdded));
        Assert.Contains(commentEvents, e => e.Id == event1.Id);
        Assert.DoesNotContain(commentEvents, e => e.Id == event2.Id);

        // Act & Assert - Filter by fromTimestamp
        var recentEvents = await eventStore.ReadEventsAsync(fromTimestamp: baseTime.AddMinutes(-6));
        var recentList = recentEvents.ToList();
        Assert.Contains(recentList, e => e.Id == event2.Id);
        Assert.Contains(recentList, e => e.Id == event3.Id);

        // Act & Assert - Filter by toTimestamp
        var oldEvents = await eventStore.ReadEventsAsync(toTimestamp: baseTime.AddMinutes(-6));
        Assert.Contains(oldEvents, e => e.Id == event1.Id);
        Assert.DoesNotContain(oldEvents, e => e.Id == event2.Id);

        // Act & Assert - Combined filters
        var specificEvents = await eventStore.ReadEventsAsync(
            eventType: nameof(CommentAdded),
            fromTimestamp: baseTime.AddMinutes(-15),
            toTimestamp: baseTime.AddMinutes(-5));
        Assert.Contains(specificEvents, e => e.Id == event1.Id);
        Assert.DoesNotContain(specificEvents, e => e.Id == event2.Id);
    }

    [Fact]
    public async Task AppendAsync_ConcurrentWrites_ThreadSafe()
    {
        // Arrange
        var eventStore = new JsonFileEventStore();
        var events = Enumerable.Range(0, 10)
            .Select(i => TestHelpers.CreateCommentAddedEvent(text: $"Concurrent {i}"))
            .ToList();

        // Act - Write concurrently
        var tasks = events.Select(e => eventStore.AppendAsync(e)).ToList();
        await Task.WhenAll(tasks);

        // Assert - All events should be persisted
        var allEvents = await eventStore.GetAllAsync();
        foreach (var @event in events)
        {
            Assert.Contains(allEvents, e => e.Id == @event.Id);
        }
    }

    [Fact]
    public async Task ReadAllEventsAsync_AliasForGetAllAsync()
    {
        // Arrange
        var eventStore = new JsonFileEventStore();
        var @event = TestHelpers.CreateCommentAddedEvent();
        await eventStore.AppendAsync(@event);

        // Act
        var allEvents1 = await eventStore.GetAllAsync();
        var allEvents2 = await eventStore.ReadAllEventsAsync();

        // Assert - Both methods should return same results
        var list1 = allEvents1.ToList();
        var list2 = allEvents2.ToList();
        Assert.Equal(list1.Count, list2.Count);
    }

    [Fact]
    public async Task AppendEventAsync_AliasForAppendAsync()
    {
        // Arrange
        var eventStore = new JsonFileEventStore();
        var @event = TestHelpers.CreateCommentAddedEvent();

        // Act
        await eventStore.AppendEventAsync(@event);

        // Assert
        var allEvents = await eventStore.GetAllAsync();
        Assert.Contains(allEvents, e => e.Id == @event.Id);
    }
}
