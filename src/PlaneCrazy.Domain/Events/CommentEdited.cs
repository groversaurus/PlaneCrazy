namespace PlaneCrazy.Domain.Events;

/// <summary>
/// Event raised when a comment is edited.
/// </summary>
public class CommentEdited : DomainEvent
{
    /// <summary>
    /// The type of entity the comment is associated with (e.g., "Aircraft", "Type", "Airport").
    /// </summary>
    public required string EntityType { get; init; }
    
    /// <summary>
    /// The identifier of the entity the comment is associated with.
    /// </summary>
    public required string EntityId { get; init; }
    
    /// <summary>
    /// The unique identifier of the comment being edited.
    /// </summary>
    public required Guid CommentId { get; init; }
    
    /// <summary>
    /// The new text content of the comment.
    /// </summary>
    public required string Text { get; init; }
    
    /// <summary>
    /// The username or identifier of the user who edited the comment.
    /// </summary>
    public string? User { get; init; }
    
    /// <summary>
    /// The timestamp when the comment was edited. Defaults to event occurrence time.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// The previous text content before the edit (optional, for audit trail).
    /// </summary>
    public string? PreviousText { get; init; }
    
    public override string? GetEntityType() => EntityType;
    public override string? GetEntityId() => EntityId;
}
