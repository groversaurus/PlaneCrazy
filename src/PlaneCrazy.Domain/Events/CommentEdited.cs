namespace PlaneCrazy.Domain.Events;

/// <summary>
/// Event raised when a comment is edited.
/// </summary>
public class CommentEdited
{
    /// <summary>
    /// Gets or sets the type of entity the comment is associated with.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the entity the comment is associated with.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the comment.
    /// </summary>
    public string CommentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated text content of the comment.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user who edited the comment.
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the comment was edited.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
