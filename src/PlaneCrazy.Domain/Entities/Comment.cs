namespace PlaneCrazy.Domain.Entities;

public class Comment
{
    /// <summary>
    /// Unique identifier for the comment.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// The type of entity this comment is associated with (e.g., "Aircraft", "Type", "Airport").
    /// </summary>
    public required string EntityType { get; init; }
    
    /// <summary>
    /// The identifier of the entity this comment is associated with.
    /// </summary>
    public required string EntityId { get; init; }
    
    /// <summary>
    /// The current text content of the comment.
    /// </summary>
    public required string Text { get; set; }
    
    /// <summary>
    /// When the comment was originally created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// The user who created the comment.
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// When the comment was last updated (null if never edited).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// The user who last updated the comment.
    /// </summary>
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// Indicates if the comment has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// When the comment was deleted (if applicable).
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// The user who deleted the comment.
    /// </summary>
    public string? DeletedBy { get; set; }
    
    /// <summary>
    /// The reason for deletion (if applicable).
    /// </summary>
    public string? DeletionReason { get; set; }
}
