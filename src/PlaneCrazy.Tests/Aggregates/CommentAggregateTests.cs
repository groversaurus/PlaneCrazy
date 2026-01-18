using PlaneCrazy.Domain.Aggregates;
using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Tests.Helpers;

namespace PlaneCrazy.Tests.Aggregates;

/// <summary>
/// Tests for CommentAggregate event sourcing logic.
/// </summary>
public class CommentAggregateTests
{
    [Fact]
    public void AddComment_ValidCommand_RaisesCommentAddedEvent()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate(commentId);
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Great plane!",
            User = "testuser",
            IssuedBy = "testuser"
        };

        // Act
        aggregate.AddComment(command);

        // Assert
        var uncommittedEvents = aggregate.GetUncommittedEvents().ToList();
        Assert.Single(uncommittedEvents);
        
        var @event = uncommittedEvents[0] as CommentAdded;
        Assert.NotNull(@event);
        Assert.Equal(commentId, @event.CommentId);
        Assert.Equal("Aircraft", @event.EntityType);
        Assert.Equal("ABC123", @event.EntityId);
        Assert.Equal("Great plane!", @event.Text);
        Assert.Equal("testuser", @event.User);
    }

    [Fact]
    public void EditComment_ExistingComment_RaisesCommentEditedEvent()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate(commentId);
        
        // Add a comment first
        var addedEvent = TestHelpers.CreateCommentAddedEvent(
            commentId: commentId,
            text: "Original text");
        aggregate.LoadFromHistory(new[] { addedEvent });
        aggregate.MarkEventsAsCommitted();

        var command = new EditCommentCommand
        {
            CommentId = commentId,
            EntityType = "Aircraft",
            EntityId = "A12345",
            NewText = "Updated text",
            User = "testuser",
            IssuedBy = "testuser"
        };

        // Act
        aggregate.EditComment(command);

        // Assert
        var uncommittedEvents = aggregate.GetUncommittedEvents().ToList();
        Assert.Single(uncommittedEvents);
        
        var @event = uncommittedEvents[0] as CommentEdited;
        Assert.NotNull(@event);
        Assert.Equal(commentId, @event.CommentId);
        Assert.Equal("Updated text", @event.Text);
        Assert.Equal("Original text", @event.PreviousText);
    }

    [Fact]
    public void EditComment_DeletedComment_ThrowsException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate(commentId);
        
        // Add and then delete a comment
        var addedEvent = TestHelpers.CreateCommentAddedEvent(commentId: commentId);
        var deletedEvent = TestHelpers.CreateCommentDeletedEvent(commentId);
        aggregate.LoadFromHistory(new DomainEvent[] { addedEvent, deletedEvent });
        aggregate.MarkEventsAsCommitted();

        var command = new EditCommentCommand
        {
            CommentId = commentId,
            EntityType = "Aircraft",
            EntityId = "A12345",
            NewText = "Trying to edit",
            User = "testuser",
            IssuedBy = "testuser"
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => aggregate.EditComment(command));
        Assert.Contains("Cannot edit a deleted comment", exception.Message);
    }

    [Fact]
    public void DeleteComment_ExistingComment_RaisesCommentDeletedEvent()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate(commentId);
        
        // Add a comment first
        var addedEvent = TestHelpers.CreateCommentAddedEvent(commentId: commentId);
        aggregate.LoadFromHistory(new[] { addedEvent });
        aggregate.MarkEventsAsCommitted();

        var command = new DeleteCommentCommand
        {
            CommentId = commentId,
            EntityType = "Aircraft",
            EntityId = "A12345",
            Reason = "Spam",
            User = "moderator",
            IssuedBy = "moderator"
        };

        // Act
        aggregate.DeleteComment(command);

        // Assert
        var uncommittedEvents = aggregate.GetUncommittedEvents().ToList();
        Assert.Single(uncommittedEvents);
        
        var @event = uncommittedEvents[0] as CommentDeleted;
        Assert.NotNull(@event);
        Assert.Equal(commentId, @event.CommentId);
        Assert.Equal("Spam", @event.Reason);
        Assert.Equal("moderator", @event.User);
    }

    [Fact]
    public void DeleteComment_AlreadyDeleted_ThrowsException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate(commentId);
        
        // Add and then delete a comment
        var addedEvent = TestHelpers.CreateCommentAddedEvent(commentId: commentId);
        var deletedEvent = TestHelpers.CreateCommentDeletedEvent(commentId);
        aggregate.LoadFromHistory(new DomainEvent[] { addedEvent, deletedEvent });
        aggregate.MarkEventsAsCommitted();

        var command = new DeleteCommentCommand
        {
            CommentId = commentId,
            EntityType = "Aircraft",
            EntityId = "A12345",
            Reason = "Trying again",
            User = "testuser",
            IssuedBy = "testuser"
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => aggregate.DeleteComment(command));
        Assert.Contains("already deleted", exception.Message);
    }

    [Fact]
    public void LoadFromHistory_RebuildsState()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate();
        
        // Create event stream: CommentAdded → CommentEdited → CommentEdited
        var events = new List<DomainEvent>
        {
            TestHelpers.CreateCommentAddedEvent(
                commentId: commentId,
                text: "Original",
                timestamp: DateTime.UtcNow.AddMinutes(-10)),
            TestHelpers.CreateCommentEditedEvent(
                commentId: commentId,
                text: "First edit",
                previousText: "Original",
                timestamp: DateTime.UtcNow.AddMinutes(-5)),
            TestHelpers.CreateCommentEditedEvent(
                commentId: commentId,
                text: "Second edit",
                previousText: "First edit",
                timestamp: DateTime.UtcNow)
        };

        // Act
        aggregate.LoadFromHistory(events);

        // Assert
        Assert.Equal(commentId.ToString(), aggregate.Id);
        Assert.Equal(3, aggregate.Version); // 3 events applied
        Assert.False(aggregate.IsDeleted);
    }

    [Fact]
    public void LoadFromHistory_WithDeletion_MarksAsDeleted()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate();
        
        // Create event stream with deletion
        var events = new List<DomainEvent>
        {
            TestHelpers.CreateCommentAddedEvent(commentId: commentId),
            TestHelpers.CreateCommentEditedEvent(commentId, text: "Edited"),
            TestHelpers.CreateCommentDeletedEvent(commentId, reason: "Inappropriate")
        };

        // Act
        aggregate.LoadFromHistory(events);

        // Assert
        Assert.Equal(commentId.ToString(), aggregate.Id);
        Assert.Equal(3, aggregate.Version);
        Assert.True(aggregate.IsDeleted);
    }

    [Fact]
    public void MarkEventsAsCommitted_ClearsUncommittedEvents()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate(commentId);
        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Test",
            User = "testuser",
            IssuedBy = "testuser"
        };

        // Act
        aggregate.AddComment(command);
        Assert.Single(aggregate.GetUncommittedEvents()); // Verify event exists
        
        aggregate.MarkEventsAsCommitted();

        // Assert
        Assert.Empty(aggregate.GetUncommittedEvents());
    }

    [Fact]
    public void AddComment_OnExistingAggregate_ThrowsException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate(commentId);
        
        // Add a comment first
        var addedEvent = TestHelpers.CreateCommentAddedEvent(commentId: commentId);
        aggregate.LoadFromHistory(new[] { addedEvent });

        var command = new AddCommentCommand
        {
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Another comment",
            User = "testuser",
            IssuedBy = "testuser"
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => aggregate.AddComment(command));
        Assert.Contains("Comment already exists", exception.Message);
    }
}
