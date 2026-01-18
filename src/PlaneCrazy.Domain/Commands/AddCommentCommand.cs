namespace PlaneCrazy.Domain.Commands;

/// <summary>
/// Command to add a new comment to an entity.
/// </summary>
public class AddCommentCommand : Command
{
    /// <summary>
    /// The type of entity to comment on (e.g., "Aircraft", "Type", "Airport").
    /// </summary>
    public required string EntityType { get; init; }
    
    /// <summary>
    /// The identifier of the entity to comment on.
    /// </summary>
    public required string EntityId { get; init; }
    
    /// <summary>
    /// The text content of the comment.
    /// </summary>
    public required string Text { get; init; }
    
    /// <summary>
    /// The user adding the comment (optional, falls back to IssuedBy).
    /// </summary>
    public string? User { get; init; }
    
    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(EntityType))
            throw new ArgumentException("EntityType cannot be empty.", nameof(EntityType));
        
        if (string.IsNullOrWhiteSpace(EntityId))
            throw new ArgumentException("EntityId cannot be empty.", nameof(EntityId));
        
        if (string.IsNullOrWhiteSpace(Text))
            throw new ArgumentException("Text cannot be empty.", nameof(Text));
        
        if (Text.Length > 5000)
            throw new ArgumentException("Text cannot exceed 5000 characters.", nameof(Text));
    }
}
