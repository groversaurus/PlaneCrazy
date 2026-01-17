using PlaneCrazy.Core.Comments;
using PlaneCrazy.Core.Events;
using PlaneCrazy.Core.Projections;

namespace PlaneCrazy.Tests;

public class CommentsProjectionTests
{
    private const string EntityId = "test-entity-123";

    [Fact]
    public void RebuildFromEvents_WithNoEvents_ReturnsEmptyCommentList()
    {
        // Arrange
        var projection = new CommentsProjection();
        var events = new List<IEvent>();

        // Act
        projection.RebuildFromEvents(events);
        var comments = projection.GetComments(EntityId);

        // Assert
        Assert.Empty(comments);
    }

    [Fact]
    public void RebuildFromEvents_WithCommentAddedEvent_CreatesComment()
    {
        // Arrange
        var projection = new CommentsProjection();
        var commentId = Guid.NewGuid();
        var events = new List<IEvent>
        {
            new CommentAddedEvent
            {
                CommentId = commentId,
                EntityId = EntityId,
                Text = "Test comment",
                Author = "John Doe",
                Timestamp = DateTime.UtcNow
            }
        };

        // Act
        projection.RebuildFromEvents(events);
        var comments = projection.GetComments(EntityId);

        // Assert
        Assert.Single(comments);
        var comment = comments.First();
        Assert.Equal(commentId, comment.CommentId);
        Assert.Equal(EntityId, comment.EntityId);
        Assert.Equal("Test comment", comment.Text);
        Assert.Equal("John Doe", comment.Author);
        Assert.False(comment.IsDeleted);
    }

    [Fact]
    public void RebuildFromEvents_WithMultipleCommentAddedEvents_CreatesMultipleComments()
    {
        // Arrange
        var projection = new CommentsProjection();
        var commentId1 = Guid.NewGuid();
        var commentId2 = Guid.NewGuid();
        var events = new List<IEvent>
        {
            new CommentAddedEvent
            {
                CommentId = commentId1,
                EntityId = EntityId,
                Text = "First comment",
                Author = "John Doe",
                Timestamp = DateTime.UtcNow.AddMinutes(-5)
            },
            new CommentAddedEvent
            {
                CommentId = commentId2,
                EntityId = EntityId,
                Text = "Second comment",
                Author = "Jane Smith",
                Timestamp = DateTime.UtcNow
            }
        };

        // Act
        projection.RebuildFromEvents(events);
        var comments = projection.GetComments(EntityId);

        // Assert
        Assert.Equal(2, comments.Count);
        Assert.Equal("First comment", comments.First().Text);
        Assert.Equal("Second comment", comments.Last().Text);
    }

    [Fact]
    public void RebuildFromEvents_WithCommentUpdatedEvent_UpdatesCommentText()
    {
        // Arrange
        var projection = new CommentsProjection();
        var commentId = Guid.NewGuid();
        var events = new List<IEvent>
        {
            new CommentAddedEvent
            {
                CommentId = commentId,
                EntityId = EntityId,
                Text = "Original text",
                Author = "John Doe",
                Timestamp = DateTime.UtcNow.AddMinutes(-5)
            },
            new CommentUpdatedEvent
            {
                CommentId = commentId,
                EntityId = EntityId,
                Text = "Updated text",
                Timestamp = DateTime.UtcNow
            }
        };

        // Act
        projection.RebuildFromEvents(events);
        var comment = projection.GetComment(commentId);

        // Assert
        Assert.NotNull(comment);
        Assert.Equal("Updated text", comment.Text);
        Assert.NotNull(comment.UpdatedAt);
    }

    [Fact]
    public void RebuildFromEvents_WithCommentDeletedEvent_MarksCommentAsDeleted()
    {
        // Arrange
        var projection = new CommentsProjection();
        var commentId = Guid.NewGuid();
        var events = new List<IEvent>
        {
            new CommentAddedEvent
            {
                CommentId = commentId,
                EntityId = EntityId,
                Text = "Test comment",
                Author = "John Doe",
                Timestamp = DateTime.UtcNow.AddMinutes(-5)
            },
            new CommentDeletedEvent
            {
                CommentId = commentId,
                EntityId = EntityId,
                Timestamp = DateTime.UtcNow
            }
        };

        // Act
        projection.RebuildFromEvents(events);
        var comments = projection.GetComments(EntityId);
        var comment = projection.GetComment(commentId);

        // Assert
        Assert.Empty(comments); // GetComments should exclude deleted comments
        Assert.Null(comment); // GetComment should return null for deleted comments
    }

    [Fact]
    public void RebuildFromEvents_WithEventsInWrongOrder_ProcessesInCorrectOrder()
    {
        // Arrange
        var projection = new CommentsProjection();
        var commentId = Guid.NewGuid();
        var addedTimestamp = DateTime.UtcNow.AddMinutes(-10);
        var updatedTimestamp = DateTime.UtcNow.AddMinutes(-5);
        
        // Events are added in reverse chronological order
        var events = new List<IEvent>
        {
            new CommentUpdatedEvent
            {
                CommentId = commentId,
                EntityId = EntityId,
                Text = "Updated text",
                Timestamp = updatedTimestamp
            },
            new CommentAddedEvent
            {
                CommentId = commentId,
                EntityId = EntityId,
                Text = "Original text",
                Author = "John Doe",
                Timestamp = addedTimestamp
            }
        };

        // Act
        projection.RebuildFromEvents(events);
        var comment = projection.GetComment(commentId);

        // Assert
        Assert.NotNull(comment);
        Assert.Equal("Updated text", comment.Text); // Should have the updated text
        Assert.Equal("John Doe", comment.Author); // Should have the author from add event
    }

    [Fact]
    public void RebuildFromEvents_WithMultipleEntities_ReturnsCorrectCommentsForEachEntity()
    {
        // Arrange
        var projection = new CommentsProjection();
        var entity1Id = "entity-1";
        var entity2Id = "entity-2";
        var comment1Id = Guid.NewGuid();
        var comment2Id = Guid.NewGuid();
        
        var events = new List<IEvent>
        {
            new CommentAddedEvent
            {
                CommentId = comment1Id,
                EntityId = entity1Id,
                Text = "Comment for entity 1",
                Author = "John Doe",
                Timestamp = DateTime.UtcNow
            },
            new CommentAddedEvent
            {
                CommentId = comment2Id,
                EntityId = entity2Id,
                Text = "Comment for entity 2",
                Author = "Jane Smith",
                Timestamp = DateTime.UtcNow
            }
        };

        // Act
        projection.RebuildFromEvents(events);
        var entity1Comments = projection.GetComments(entity1Id);
        var entity2Comments = projection.GetComments(entity2Id);

        // Assert
        Assert.Single(entity1Comments);
        Assert.Single(entity2Comments);
        Assert.Equal("Comment for entity 1", entity1Comments.First().Text);
        Assert.Equal("Comment for entity 2", entity2Comments.First().Text);
    }

    [Fact]
    public void RebuildFromEvents_CalledMultipleTimes_ClearsPreviousState()
    {
        // Arrange
        var projection = new CommentsProjection();
        var commentId1 = Guid.NewGuid();
        var commentId2 = Guid.NewGuid();
        
        var firstEvents = new List<IEvent>
        {
            new CommentAddedEvent
            {
                CommentId = commentId1,
                EntityId = EntityId,
                Text = "First comment",
                Author = "John Doe",
                Timestamp = DateTime.UtcNow
            }
        };

        var secondEvents = new List<IEvent>
        {
            new CommentAddedEvent
            {
                CommentId = commentId2,
                EntityId = EntityId,
                Text = "Second comment",
                Author = "Jane Smith",
                Timestamp = DateTime.UtcNow
            }
        };

        // Act
        projection.RebuildFromEvents(firstEvents);
        var firstComments = projection.GetComments(EntityId);
        
        projection.RebuildFromEvents(secondEvents);
        var secondComments = projection.GetComments(EntityId);

        // Assert
        Assert.Single(firstComments);
        Assert.Equal("First comment", firstComments.First().Text);
        
        Assert.Single(secondComments);
        Assert.Equal("Second comment", secondComments.First().Text);
    }

    [Fact]
    public void GetComments_ReturnsCommentsOrderedByCreatedAt()
    {
        // Arrange
        var projection = new CommentsProjection();
        var timestamp1 = DateTime.UtcNow.AddMinutes(-10);
        var timestamp2 = DateTime.UtcNow.AddMinutes(-5);
        var timestamp3 = DateTime.UtcNow;
        
        var events = new List<IEvent>
        {
            new CommentAddedEvent
            {
                CommentId = Guid.NewGuid(),
                EntityId = EntityId,
                Text = "Third comment",
                Author = "User 3",
                Timestamp = timestamp3
            },
            new CommentAddedEvent
            {
                CommentId = Guid.NewGuid(),
                EntityId = EntityId,
                Text = "First comment",
                Author = "User 1",
                Timestamp = timestamp1
            },
            new CommentAddedEvent
            {
                CommentId = Guid.NewGuid(),
                EntityId = EntityId,
                Text = "Second comment",
                Author = "User 2",
                Timestamp = timestamp2
            }
        };

        // Act
        projection.RebuildFromEvents(events);
        var comments = projection.GetComments(EntityId);

        // Assert
        Assert.Equal(3, comments.Count);
        Assert.Equal("First comment", comments.ElementAt(0).Text);
        Assert.Equal("Second comment", comments.ElementAt(1).Text);
        Assert.Equal("Third comment", comments.ElementAt(2).Text);
    }
}
