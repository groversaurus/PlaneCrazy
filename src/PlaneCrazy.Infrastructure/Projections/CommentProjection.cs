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

    public async Task RebuildAsync()
    {
        var events = await _eventStore.GetAllAsync();
        
        foreach (var @event in events)
        {
            await ApplyEventAsync(@event);
        }
    }

    private async Task ApplyEventAsync(DomainEvent @event)
    {
        switch (@event)
        {
            case CommentAdded commentAdded:
                await _commentRepository.SaveAsync(new Domain.Entities.Comment
                {
                    Id = commentAdded.CommentId,
                    EntityType = commentAdded.EntityType,
                    EntityId = commentAdded.EntityId,
                    Text = commentAdded.Text,
                    CreatedAt = commentAdded.Timestamp
                });
                break;
                
            case CommentEdited commentEdited:
                var existingComment = await _commentRepository.GetByIdAsync(commentEdited.CommentId.ToString());
                if (existingComment != null)
                {
                    existingComment.Text = commentEdited.Text;
                    existingComment.UpdatedAt = commentEdited.Timestamp;
                    existingComment.UpdatedBy = commentEdited.User;
                    await _commentRepository.SaveAsync(existingComment);
                }
                break;
                
            case CommentDeleted commentDeleted:
                await _commentRepository.DeleteAsync(commentDeleted.CommentId.ToString());
                break;
        }
    }
}
