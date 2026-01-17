using PlaneCrazy.Domain.Events;
using Xunit;

namespace PlaneCrazy.Domain.Tests.Events;

public class CommentAddedTests
{
    [Fact]
    public void CommentAdded_CanBeInstantiated()
    {
        // Arrange & Act
        var commentAdded = new CommentAdded();

        // Assert
        Assert.NotNull(commentAdded);
    }

    [Fact]
    public void CommentAdded_PropertiesCanBeSet()
    {
        // Arrange
        var entityType = "Aircraft";
        var entityId = "ABC123";
        var commentId = "comment-001";
        var text = "Flight looks smooth";
        var user = "john.doe@example.com";
        var timestamp = DateTime.UtcNow;

        // Act
        var commentAdded = new CommentAdded
        {
            EntityType = entityType,
            EntityId = entityId,
            CommentId = commentId,
            Text = text,
            User = user,
            Timestamp = timestamp
        };

        // Assert
        Assert.Equal(entityType, commentAdded.EntityType);
        Assert.Equal(entityId, commentAdded.EntityId);
        Assert.Equal(commentId, commentAdded.CommentId);
        Assert.Equal(text, commentAdded.Text);
        Assert.Equal(user, commentAdded.User);
        Assert.Equal(timestamp, commentAdded.Timestamp);
    }

    [Fact]
    public void CommentAdded_DefaultValuesAreInitialized()
    {
        // Arrange & Act
        var commentAdded = new CommentAdded();

        // Assert
        Assert.Equal(string.Empty, commentAdded.EntityType);
        Assert.Equal(string.Empty, commentAdded.EntityId);
        Assert.Equal(string.Empty, commentAdded.CommentId);
        Assert.Equal(string.Empty, commentAdded.Text);
        Assert.Equal(string.Empty, commentAdded.User);
        Assert.Equal(default(DateTime), commentAdded.Timestamp);
    }
}
