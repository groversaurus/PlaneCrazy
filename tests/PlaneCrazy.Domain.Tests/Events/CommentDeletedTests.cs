using PlaneCrazy.Domain.Events;
using Xunit;

namespace PlaneCrazy.Domain.Tests.Events;

public class CommentDeletedTests
{
    [Fact]
    public void CommentDeleted_CanBeInstantiated()
    {
        // Arrange & Act
        var commentDeleted = new CommentDeleted();

        // Assert
        Assert.NotNull(commentDeleted);
    }

    [Fact]
    public void CommentDeleted_PropertiesCanBeSet()
    {
        // Arrange
        var entityType = "Aircraft";
        var entityId = "ABC123";
        var commentId = "comment-001";
        var text = "Flight looks smooth";
        var user = "admin@example.com";
        var timestamp = DateTime.UtcNow;

        // Act
        var commentDeleted = new CommentDeleted
        {
            EntityType = entityType,
            EntityId = entityId,
            CommentId = commentId,
            Text = text,
            User = user,
            Timestamp = timestamp
        };

        // Assert
        Assert.Equal(entityType, commentDeleted.EntityType);
        Assert.Equal(entityId, commentDeleted.EntityId);
        Assert.Equal(commentId, commentDeleted.CommentId);
        Assert.Equal(text, commentDeleted.Text);
        Assert.Equal(user, commentDeleted.User);
        Assert.Equal(timestamp, commentDeleted.Timestamp);
    }

    [Fact]
    public void CommentDeleted_DefaultValuesAreInitialized()
    {
        // Arrange & Act
        var commentDeleted = new CommentDeleted();

        // Assert
        Assert.Equal(string.Empty, commentDeleted.EntityType);
        Assert.Equal(string.Empty, commentDeleted.EntityId);
        Assert.Equal(string.Empty, commentDeleted.CommentId);
        Assert.Equal(string.Empty, commentDeleted.Text);
        Assert.Equal(string.Empty, commentDeleted.User);
        Assert.Equal(default(DateTime), commentDeleted.Timestamp);
    }
}
