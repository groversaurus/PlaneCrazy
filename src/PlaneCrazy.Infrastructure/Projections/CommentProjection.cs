using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.Repositories;

namespace PlaneCrazy.Infrastructure.Projections;

public class CommentProjection
{
    private readonly IEventStore _eventStore;
    private readonly CommentRepository _commentRepository;

    public CommentProjection(IEventStore eventStore, CommentRepository commentRepository)
    {
        _eventStore = eventStore;
        _commentRepository = commentRepository;
    }

    /// <summary>
    /// Rebuilds all comments from all events in the event store.
    /// Clears existing comments and replays all events.
    /// </summary>
    public async Task RebuildAsync()
    {
        // Clear all existing comments before rebuilding
        var allComments = await _commentRepository.GetAllAsync();
        foreach (var comment in allComments)
        {
            await _commentRepository.DeleteAsync(comment.Id.ToString());
        }
        
        // Get all events and replay them
        var events = await _eventStore.GetAllAsync();
        
        foreach (var @event in events)
        {
            await ApplyEventAsync(@event);
        }
    }

    /// <summary>
    /// Rebuilds comments for a specific entity by replaying only its comment events.
    /// </summary>
    /// <param name="entityType">The type of entity (e.g., "Aircraft", "Type", "Airport").</param>
    /// <param name="entityId">The specific entity identifier.</param>
    public async Task RebuildForEntityAsync(string entityType, string entityId)
    {
        // Clear existing comments for this entity
        var existingComments = await _commentRepository.GetByEntityAsync(entityType, entityId);
        foreach (var comment in existingComments)
        {
            await _commentRepository.DeleteAsync(comment.Id.ToString());
        }
        
        // Get events for this specific entity using the hierarchical event store
        // Note: This assumes the event store has a GetByEntityAsync method
        // If not available, we filter from GetAllAsync
        var allEvents = await _eventStore.GetAllAsync();
        var entityEvents = allEvents.Where(e => 
            (e is CommentAdded ca && ca.EntityType == entityType && ca.EntityId == entityId) ||
            (e is CommentEdited ce && ce.EntityType == entityType && ce.EntityId == entityId) ||
            (e is CommentDeleted cd && cd.EntityType == entityType && cd.EntityId == entityId))
            .OrderBy(e => e.OccurredAt);
        
        // Replay events in chronological order
        foreach (var @event in entityEvents)
        {
            await ApplyEventAsync(@event);
        }
    }

    /// <summary>
    /// Applies a single domain event to update the comment projection.
    /// </summary>
    private async Task ApplyEventAsync(DomainEvent @event)
    {
        switch (@event)
        {
            case CommentAdded commentAdded:
                await HandleCommentAddedAsync(commentAdded);
                break;
                
            case CommentEdited commentEdited:
                await HandleCommentEditedAsync(commentEdited);
                break;
                
            case CommentDeleted commentDeleted:
                await HandleCommentDeletedAsync(commentDeleted);
                break;
        }
    }

    private async Task HandleCommentAddedAsync(CommentAdded commentAdded)
    {
        var comment = new Domain.Entities.Comment
        {
            Id = commentAdded.CommentId,
            EntityType = commentAdded.EntityType,
            EntityId = commentAdded.EntityId,
            Text = commentAdded.Text,
            CreatedAt = commentAdded.Timestamp,
            CreatedBy = commentAdded.User,
            IsDeleted = false
        };
        
        await _commentRepository.SaveAsync(comment);
    }

    private async Task HandleCommentEditedAsync(CommentEdited commentEdited)
    {
        var existingComment = await _commentRepository.GetByIdAsync(commentEdited.CommentId.ToString());
        
        if (existingComment != null)
        {
            existingComment.Text = commentEdited.Text;
            existingComment.UpdatedAt = commentEdited.Timestamp;
            existingComment.UpdatedBy = commentEdited.User;
            
            await _commentRepository.SaveAsync(existingComment);
        }
        // If comment doesn't exist, we might want to log this as a data inconsistency
    }

    private async Task HandleCommentDeletedAsync(CommentDeleted commentDeleted)
    {
        var existingComment = await _commentRepository.GetByIdAsync(commentDeleted.CommentId.ToString());
        
        if (existingComment != null)
        {
            // Soft delete - mark as deleted but keep the record
            existingComment.IsDeleted = true;
            existingComment.DeletedAt = commentDeleted.Timestamp;
            existingComment.DeletedBy = commentDeleted.User;
            existingComment.DeletionReason = commentDeleted.Reason;
            
            await _commentRepository.SaveAsync(existingComment);
            
            // Alternative: Hard delete - remove the record completely
            // await _commentRepository.DeleteAsync(commentDeleted.CommentId.ToString());
        }
    }
}
