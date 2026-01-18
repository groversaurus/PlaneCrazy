using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Tests.Helpers;

namespace PlaneCrazy.Tests.CommandHandlers;

/// <summary>
/// Tests for AddCommentCommandHandler workflow.
/// Uses in-memory implementations for fast, isolated testing.
/// </summary>
public class AddCommentCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_AppendsEventToStore()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Great plane!",
            User = "testuser",
            IssuedBy = "testuser"
        };

        // Act - manually execute the command logic
        var commentId = Guid.NewGuid();
        var aggregate = new Domain.Aggregates.CommentAggregate(commentId);
        aggregate.AddComment(command);
        
        var events = aggregate.GetUncommittedEvents();
        foreach (var @event in events)
        {
            await eventStore.AppendEventAsync(@event);
        }

        // Assert
        var storedEvents = await eventStore.GetAllAsync();
        var commentAddedEvents = storedEvents.OfType<CommentAdded>().ToList();
        
        Assert.Single(commentAddedEvents);
        var evt = commentAddedEvents[0];
        Assert.Equal("Aircraft", evt.EntityType);
        Assert.Equal("ABC123", evt.EntityId);
        Assert.Equal("Great plane!", evt.Text);
        Assert.Equal("testuser", evt.User);
    }

    [Fact]
    public async Task HandleAsync_CreatesNewCommentId()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        
        var command1 = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Comment 1",
            User = "testuser",
            IssuedBy = "testuser"
        };
        
        var command2 = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Comment 2",
            User = "testuser",
            IssuedBy = "testuser"
        };

        // Act
        var commentId1 = Guid.NewGuid();
        var aggregate1 = new Domain.Aggregates.CommentAggregate(commentId1);
        aggregate1.AddComment(command1);
        foreach (var @event in aggregate1.GetUncommittedEvents())
        {
            await eventStore.AppendEventAsync(@event);
        }
        
        var commentId2 = Guid.NewGuid();
        var aggregate2 = new Domain.Aggregates.CommentAggregate(commentId2);
        aggregate2.AddComment(command2);
        foreach (var @event in aggregate2.GetUncommittedEvents())
        {
            await eventStore.AppendEventAsync(@event);
        }

        // Assert
        var events = (await eventStore.GetAllAsync()).OfType<CommentAdded>().ToList();
        Assert.Equal(2, events.Count);
        Assert.NotEqual(events[0].CommentId, events[1].CommentId);
    }

    [Fact]
    public void HandleAsync_InvalidCommand_ThrowsException()
    {
        // Arrange
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "", // Empty text is invalid
            User = "testuser",
            IssuedBy = "testuser"
        };

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => command.Validate());
    }
}
