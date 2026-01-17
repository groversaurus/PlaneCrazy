using PlaneCrazy.Core.Events;
using PlaneCrazy.Core.Comments;

namespace PlaneCrazy.Core.Projections;

public class CommentsProjection
{
    private readonly Dictionary<Guid, Comment> _comments = new();

    public IReadOnlyCollection<Comment> GetComments(string entityId)
    {
        return _comments.Values
            .Where(c => c.EntityId == entityId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToList()
            .AsReadOnly();
    }

    public Comment? GetComment(Guid commentId)
    {
        return _comments.TryGetValue(commentId, out var comment) && !comment.IsDeleted
            ? comment
            : null;
    }

    public void RebuildFromEvents(IEnumerable<IEvent> events)
    {
        _comments.Clear();

        foreach (var @event in events.OrderBy(e => e.Timestamp))
        {
            ApplyEvent(@event);
        }
    }

    private void ApplyEvent(IEvent @event)
    {
        switch (@event)
        {
            case CommentAddedEvent added:
                HandleCommentAdded(added);
                break;
            case CommentUpdatedEvent updated:
                HandleCommentUpdated(updated);
                break;
            case CommentDeletedEvent deleted:
                HandleCommentDeleted(deleted);
                break;
        }
    }

    private void HandleCommentAdded(CommentAddedEvent @event)
    {
        var comment = new Comment
        {
            CommentId = @event.CommentId,
            EntityId = @event.EntityId,
            Text = @event.Text,
            Author = @event.Author,
            CreatedAt = @event.Timestamp,
            IsDeleted = false
        };

        _comments[@event.CommentId] = comment;
    }

    private void HandleCommentUpdated(CommentUpdatedEvent @event)
    {
        if (_comments.TryGetValue(@event.CommentId, out var comment))
        {
            comment.Text = @event.Text;
            comment.UpdatedAt = @event.Timestamp;
        }
    }

    private void HandleCommentDeleted(CommentDeletedEvent @event)
    {
        if (_comments.TryGetValue(@event.CommentId, out var comment))
        {
            comment.IsDeleted = true;
        }
    }
}
