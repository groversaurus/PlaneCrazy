using PlaneCrazy.Domain.Queries.QueryResults;

namespace PlaneCrazy.Domain.Interfaces;

/// <summary>
/// Query service for comment read operations.
/// Uses projections only - no event store access.
/// </summary>
public interface ICommentQueryService
{
    /// <summary>
    /// Gets all comments for a specific entity.
    /// </summary>
    Task<IEnumerable<CommentQueryResult>> GetCommentsForEntityAsync(string entityType, string entityId);
    
    /// <summary>
    /// Gets only active (non-deleted) comments for an entity.
    /// </summary>
    Task<IEnumerable<CommentQueryResult>> GetActiveCommentsForEntityAsync(string entityType, string entityId);
    
    /// <summary>
    /// Gets a single comment by ID.
    /// </summary>
    Task<CommentQueryResult?> GetCommentByIdAsync(Guid commentId);
    
    /// <summary>
    /// Gets all entities that have comments.
    /// </summary>
    /// <returns>List of (EntityType, EntityId) tuples.</returns>
    Task<IEnumerable<(string EntityType, string EntityId, int CommentCount)>> GetEntitiesWithCommentsAsync();
    
    /// <summary>
    /// Gets comment count for a specific entity.
    /// </summary>
    Task<int> GetCommentCountAsync(string entityType, string entityId);
}
