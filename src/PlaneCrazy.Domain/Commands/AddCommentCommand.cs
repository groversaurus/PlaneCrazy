using PlaneCrazy.Domain.Validation;

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
        var result = CommandValidator.ValidateAddComment(this);
        if (!result.IsValid)
            throw new ValidationException(result);
    }
}
