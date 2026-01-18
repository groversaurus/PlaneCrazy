namespace PlaneCrazy.Domain.Events;

/// <summary>
/// Event raised when a comment is deleted.
/// </summary>
public class CommentDeleted : DomainEvent
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
    /// The unique identifier of the comment being deleted.
    /// </summary>
    public required Guid CommentId { get; init; }
    
    /// <summary>
    /// The username or identifier of the user who deleted the comment.
    /// </summary>
    public string? User { get; init; }
    
    /// <summary>
    /// The timestamp when the comment was deleted. Defaults to event occurrence time.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// The reason for deletion (optional).
    /// </summary>
    public string? Reason { get; init; }
    
    public override string? GetEntityType() => EntityType;
    public override string? GetEntityId() => EntityId;
}
