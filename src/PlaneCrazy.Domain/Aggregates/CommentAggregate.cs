using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Events;

namespace PlaneCrazy.Domain.Aggregates;

/// <summary>
/// Aggregate root for managing comments on entities (Aircraft, Types, Airports).
/// Maintains the state of a comment through its lifecycle and enforces business rules.
/// </summary>
public class CommentAggregate : AggregateRoot
{
    private string _entityType = string.Empty;
    private string _entityId = string.Empty;
    private string _text = string.Empty;
    private string? _createdBy;
    private DateTime _createdAt;
    private bool _isDeleted;

    /// <summary>
    /// Creates a new comment aggregate with a specific ID.
    /// </summary>
    /// <param name="commentId">The unique identifier for the comment.</param>
    public CommentAggregate(Guid commentId)
    {
        Id = commentId.ToString();
    }

    /// <summary>
    /// Creates a new comment aggregate (for loading from history).
    /// </summary>
    public CommentAggregate()
    {
    }

    /// <summary>
    /// Gets whether this comment has been deleted.
    /// </summary>
    public bool IsDeleted => _isDeleted;

    /// <summary>
    /// Handles the AddComment command, validating and creating a new comment.
    /// </summary>
    public void AddComment(AddCommentCommand command)
    {
        // Business rule: Cannot add a comment if one already exists (for new aggregates)
        if (Version > 0)
            throw new InvalidOperationException("Comment already exists.");

        var @event = new CommentAdded
        {
            EntityType = command.EntityType,
            EntityId = command.EntityId,
            CommentId = Guid.Parse(Id),
            Text = command.Text,
            User = command.User ?? command.IssuedBy,
            Timestamp = DateTime.UtcNow
        };

        ApplyChange(@event);
    }

    /// <summary>
    /// Handles the EditComment command, validating and updating the comment text.
    /// </summary>
    public void EditComment(EditCommentCommand command)
    {
        // Business rule: Cannot edit a comment that doesn't exist
        if (Version == 0)
            throw new InvalidOperationException("Comment does not exist.");

        // Business rule: Cannot edit a deleted comment
        if (_isDeleted)
            throw new InvalidOperationException("Cannot edit a deleted comment.");

        var @event = new CommentEdited
        {
            EntityType = _entityType,
            EntityId = _entityId,
            CommentId = Guid.Parse(Id),
            Text = command.NewText,
            User = command.User ?? command.IssuedBy,
            Timestamp = DateTime.UtcNow,
            PreviousText = _text
        };

        ApplyChange(@event);
    }

    /// <summary>
    /// Handles the DeleteComment command, performing a soft delete of the comment.
    /// </summary>
    public void DeleteComment(DeleteCommentCommand command)
    {
        // Business rule: Cannot delete a comment that doesn't exist
        if (Version == 0)
            throw new InvalidOperationException("Comment does not exist.");

        // Business rule: Cannot delete an already deleted comment
        if (_isDeleted)
            throw new InvalidOperationException("Comment is already deleted.");

        var @event = new CommentDeleted
        {
            EntityType = _entityType,
            EntityId = _entityId,
            CommentId = Guid.Parse(Id),
            User = command.User ?? command.IssuedBy,
            Timestamp = DateTime.UtcNow,
            Reason = command.Reason
        };

        ApplyChange(@event);
    }

    /// <summary>
    /// Applies events to rebuild the aggregate state.
    /// </summary>
    protected override void Apply(DomainEvent @event)
    {
        switch (@event)
        {
            case CommentAdded added:
                ApplyCommentAdded(added);
                break;
            case CommentEdited edited:
                ApplyCommentEdited(edited);
                break;
            case CommentDeleted deleted:
                ApplyCommentDeleted(deleted);
                break;
        }
    }

    private void ApplyCommentAdded(CommentAdded @event)
    {
        Id = @event.CommentId.ToString();
        _entityType = @event.EntityType;
        _entityId = @event.EntityId;
        _text = @event.Text;
        _createdBy = @event.User;
        _createdAt = @event.Timestamp;
        _isDeleted = false;
    }

    private void ApplyCommentEdited(CommentEdited @event)
    {
        _text = @event.Text;
    }

    private void ApplyCommentDeleted(CommentDeleted @event)
    {
        _isDeleted = true;
    }
}
