using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Infrastructure.Repositories;

public class CommentRepository : JsonFileRepository<Comment>
{
    public CommentRepository() 
        : base("comments.json")
    {
    }

    protected override string GetEntityId(Comment entity) => entity.Id.ToString();
    
    /// <summary>
    /// Gets all comments for a specific entity, ordered by creation date descending.
    /// </summary>
    public async Task<IEnumerable<Comment>> GetByEntityAsync(string entityType, string entityId)
    {
        var all = await GetAllAsync();
        return all.Where(c => c.EntityType == entityType && c.EntityId == entityId)
                  .OrderByDescending(c => c.CreatedAt);
    }
    
    /// <summary>
    /// Gets only active (non-deleted) comments for a specific entity.
    /// </summary>
    public async Task<IEnumerable<Comment>> GetActiveByEntityAsync(string entityType, string entityId)
    {
        var all = await GetAllAsync();
        return all.Where(c => c.EntityType == entityType && 
                             c.EntityId == entityId && 
                             !c.IsDeleted)
                  .OrderByDescending(c => c.CreatedAt);
    }
}
