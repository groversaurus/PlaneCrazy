using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Domain.Queries.QueryResults;
using PlaneCrazy.Infrastructure.Repositories;

namespace PlaneCrazy.Infrastructure.QueryServices;

/// <summary>
/// Query service for comment read operations using projections only.
/// </summary>
public class CommentQueryService : ICommentQueryService
{
    private readonly CommentRepository _commentRepository;

    public CommentQueryService(CommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<CommentQueryResult>> GetCommentsForEntityAsync(string entityType, string entityId)
    {
        var comments = await _commentRepository.GetByEntityAsync(entityType, entityId);
        return comments.Select(MapToQueryResult);
    }

    public async Task<IEnumerable<CommentQueryResult>> GetActiveCommentsForEntityAsync(string entityType, string entityId)
    {
        var comments = await _commentRepository.GetActiveByEntityAsync(entityType, entityId);
        return comments.Select(MapToQueryResult);
    }

    public async Task<CommentQueryResult?> GetCommentByIdAsync(Guid commentId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId.ToString());
        return comment == null ? null : MapToQueryResult(comment);
    }

    public async Task<IEnumerable<(string EntityType, string EntityId, int CommentCount)>> GetEntitiesWithCommentsAsync()
    {
        var allComments = await _commentRepository.GetAllAsync();
        
        var grouped = allComments
            .Where(c => !c.IsDeleted)
            .GroupBy(c => (c.EntityType, c.EntityId))
            .Select(g => (g.Key.EntityType, g.Key.EntityId, g.Count()))
            .OrderByDescending(x => x.Item3);

        return grouped;
    }

    public async Task<int> GetCommentCountAsync(string entityType, string entityId)
    {
        var comments = await _commentRepository.GetActiveByEntityAsync(entityType, entityId);
        return comments.Count();
    }

    private CommentQueryResult MapToQueryResult(Domain.Entities.Comment comment)
    {
        return new CommentQueryResult
        {
            Id = comment.Id,
            EntityType = comment.EntityType,
            EntityId = comment.EntityId,
            Text = comment.Text,
            CreatedBy = comment.CreatedBy,
            CreatedAt = comment.CreatedAt,
            UpdatedBy = comment.UpdatedBy,
            UpdatedAt = comment.UpdatedAt,
            IsDeleted = comment.IsDeleted,
            DeletedBy = comment.DeletedBy,
            DeletedAt = comment.DeletedAt
        };
    }
}
