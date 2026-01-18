using PlaneCrazy.Domain.Events;
using PlaneCrazy.Tests.Helpers;

namespace PlaneCrazy.Tests.EventStore;

/// <summary>
/// Tests for InMemoryEventStore implementation.
/// </summary>
public class InMemoryEventStoreTests
{
    [Fact]
    public async Task AppendAsync_ValidEvent_StoresEvent()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var @event = TestHelpers.CreateCommentAddedEvent();

        // Act
        await eventStore.AppendAsync(@event);

        // Assert
        Assert.Equal(1, eventStore.Count);
        var allEvents = await eventStore.GetAllAsync();
        Assert.Contains(allEvents, e => e.Id == @event.Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEventsInChronologicalOrder()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var event1 = TestHelpers.CreateCommentAddedEvent(timestamp: DateTime.UtcNow.AddMinutes(-2));
        var event2 = TestHelpers.CreateCommentAddedEvent(timestamp: DateTime.UtcNow.AddMinutes(-1));
        var event3 = TestHelpers.CreateCommentAddedEvent(timestamp: DateTime.UtcNow);

        // Act
        await eventStore.AppendAsync(event3);
        await eventStore.AppendAsync(event1);
        await eventStore.AppendAsync(event2);

        // Assert
        var allEvents = (await eventStore.GetAllAsync()).ToList();
        Assert.Equal(3, allEvents.Count);
        Assert.Equal(event1.Id, allEvents[0].Id);
        Assert.Equal(event2.Id, allEvents[1].Id);
        Assert.Equal(event3.Id, allEvents[2].Id);
    }

    [Fact]
    public async Task GetByTypeAsync_FiltersByEventType()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var commentEvent = TestHelpers.CreateCommentAddedEvent();
        var favouriteEvent = TestHelpers.CreateAircraftFavouritedEvent();

        await eventStore.AppendAsync(commentEvent);
        await eventStore.AppendAsync(favouriteEvent);

        // Act
        var commentEvents = await eventStore.GetByTypeAsync(nameof(CommentAdded));

        // Assert
        Assert.Single(commentEvents);
        Assert.Equal(commentEvent.Id, commentEvents.First().Id);
    }

    [Fact]
    public async Task ReadEventsAsync_FiltersByEventType()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        await eventStore.AppendAsync(TestHelpers.CreateCommentAddedEvent());
        await eventStore.AppendAsync(TestHelpers.CreateAircraftFavouritedEvent());

        // Act
        var commentEvents = await eventStore.ReadEventsAsync(eventType: nameof(CommentAdded));

        // Assert
        Assert.Single(commentEvents);
        Assert.All(commentEvents, e => Assert.Equal(nameof(CommentAdded), e.EventType));
    }

    [Fact]
    public async Task ReadEventsAsync_FiltersByTimestamp()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var baseTime = DateTime.UtcNow;
        
        // Create events with specific OccurredAt times
        var oldEvent = new CommentAdded
        {
            CommentId = Guid.NewGuid(),
            EntityType = "Aircraft",
            EntityId = "OLD",
            Text = "Old",
            User = "user",
            OccurredAt = baseTime.AddDays(-2)
        };
        
        var recentEvent = new CommentAdded
        {
            CommentId = Guid.NewGuid(),
            EntityType = "Aircraft",
            EntityId = "NEW",
            Text = "New",
            User = "user",
            OccurredAt = baseTime
        };

        await eventStore.AppendAsync(oldEvent);
        await eventStore.AppendAsync(recentEvent);

        // Act
        var recentEvents = await eventStore.ReadEventsAsync(
            fromTimestamp: baseTime.AddDays(-1));

        // Assert
        Assert.Single(recentEvents);
        Assert.Equal(recentEvent.Id, recentEvents.First().Id);
    }

    [Fact]
    public async Task Clear_RemovesAllEvents()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        await eventStore.AppendAsync(TestHelpers.CreateCommentAddedEvent());
        await eventStore.AppendAsync(TestHelpers.CreateCommentAddedEvent());
        Assert.Equal(2, eventStore.Count);

        // Act
        eventStore.Clear();

        // Assert
        Assert.Equal(0, eventStore.Count);
        var allEvents = await eventStore.GetAllAsync();
        Assert.Empty(allEvents);
    }

    [Fact]
    public async Task AppendEventAsync_AliasForAppendAsync()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var @event = TestHelpers.CreateCommentAddedEvent();

        // Act
        await eventStore.AppendEventAsync(@event);

        // Assert
        Assert.Equal(1, eventStore.Count);
    }

    [Fact]
    public async Task ReadAllEventsAsync_ReturnsAll()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        await eventStore.AppendAsync(TestHelpers.CreateCommentAddedEvent());
        await eventStore.AppendAsync(TestHelpers.CreateAircraftFavouritedEvent());

        // Act
        var allEvents1 = await eventStore.GetAllAsync();
        var allEvents2 = await eventStore.ReadAllEventsAsync();

        // Assert
        Assert.Equal(allEvents1.Count(), allEvents2.Count());
    }
}
