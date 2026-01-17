using PlaneCrazy.Domain.Events;
using Xunit;

namespace PlaneCrazy.Domain.Tests.Events;

public class CommentEditedTests
{
    [Fact]
    public void CommentEdited_CanBeInstantiated()
    {
        // Arrange & Act
        var commentEdited = new CommentEdited();

        // Assert
        Assert.NotNull(commentEdited);
    }

    [Fact]
    public void CommentEdited_PropertiesCanBeSet()
    {
        // Arrange
        var entityType = "Aircraft";
        var entityId = "ABC123";
        var commentId = "comment-001";
        var text = "Flight looks very smooth (updated)";
        var user = "john.doe@example.com";
        var timestamp = DateTime.UtcNow;

        // Act
        var commentEdited = new CommentEdited
        {
            EntityType = entityType,
            EntityId = entityId,
            CommentId = commentId,
            Text = text,
            User = user,
            Timestamp = timestamp
        };

        // Assert
        Assert.Equal(entityType, commentEdited.EntityType);
        Assert.Equal(entityId, commentEdited.EntityId);
        Assert.Equal(commentId, commentEdited.CommentId);
        Assert.Equal(text, commentEdited.Text);
        Assert.Equal(user, commentEdited.User);
        Assert.Equal(timestamp, commentEdited.Timestamp);
    }

    [Fact]
    public void CommentEdited_DefaultValuesAreInitialized()
    {
        // Arrange & Act
        var commentEdited = new CommentEdited();

        // Assert
        Assert.Equal(string.Empty, commentEdited.EntityType);
        Assert.Equal(string.Empty, commentEdited.EntityId);
        Assert.Equal(string.Empty, commentEdited.CommentId);
        Assert.Equal(string.Empty, commentEdited.Text);
        Assert.Equal(string.Empty, commentEdited.User);
        Assert.Equal(default(DateTime), commentEdited.Timestamp);
    }
}
