using PlaneCrazy.Domain.Validation;

namespace PlaneCrazy.Domain.Commands;

/// <summary>
/// Command to delete (soft delete) a comment.
/// </summary>
public class DeleteCommentCommand : Command
{
    /// <summary>
    /// The ID of the comment to delete.
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
    /// The reason for deletion (optional).
    /// </summary>
    public string? Reason { get; init; }
    
    /// <summary>
    /// The user deleting the comment (optional, falls back to IssuedBy).
    /// </summary>
    public string? User { get; init; }
    
    public override void Validate()
    {
        var result = CommandValidator.ValidateDeleteComment(this);
        if (!result.IsValid)
            throw new ValidationException(result);
    }
}
