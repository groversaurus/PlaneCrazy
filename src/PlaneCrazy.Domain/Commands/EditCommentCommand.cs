namespace PlaneCrazy.Domain.Commands;

/// <summary>
/// Command to edit an existing comment.
/// </summary>
public class EditCommentCommand : Command
{
    /// <summary>
    /// The ID of the comment to edit.
    /// </summary>
    public required Guid CommentId { get; init; }
    
    /// <summary>
    /// The type of entity the comment is on.
    /// </summary>
    public required string EntityType { get; init; }
    
    /// <summary>
    /// The identifier of the entity the comment is on.
    /// </summary>
    public required string EntityId { get; init; }
    
    /// <summary>
    /// The new text for the comment.
    /// </summary>
    public required string NewText { get; init; }
    
    /// <summary>
    /// The previous text (optional, for audit trail).
    /// </summary>
    public string? PreviousText { get; init; }
    
    /// <summary>
    /// The user editing the comment (optional, falls back to IssuedBy).
    /// </summary>
    public string? User { get; init; }
    
    public override void Validate()
    {
        if (CommentId == Guid.Empty)
            throw new ArgumentException("CommentId cannot be empty.", nameof(CommentId));
        
        if (string.IsNullOrWhiteSpace(EntityType))
            throw new ArgumentException("EntityType cannot be empty.", nameof(EntityType));
        
        if (string.IsNullOrWhiteSpace(EntityId))
            throw new ArgumentException("EntityId cannot be empty.", nameof(EntityId));
        
        if (string.IsNullOrWhiteSpace(NewText))
            throw new ArgumentException("NewText cannot be empty.", nameof(NewText));
        
        if (NewText.Length > 5000)
            throw new ArgumentException("NewText cannot exceed 5000 characters.", nameof(NewText));
    }
}
