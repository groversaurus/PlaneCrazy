using PlaneCrazy.Domain.Validation;

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
        var result = CommandValidator.ValidateEditComment(this);
        if (!result.IsValid)
            throw new ValidationException(result);
    }
}
