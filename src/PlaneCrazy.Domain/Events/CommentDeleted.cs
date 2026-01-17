namespace PlaneCrazy.Domain.Events;

/// <summary>
/// Event raised when a comment is deleted.
/// </summary>
public class CommentDeleted
{
    /// <summary>
    /// Gets or sets the type of entity the comment was associated with.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the entity the comment was associated with.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the deleted comment.
    /// </summary>
    public string CommentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text content of the deleted comment.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user who deleted the comment.
    /// </summary>
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the comment was deleted.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
