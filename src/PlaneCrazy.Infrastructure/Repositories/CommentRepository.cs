using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Infrastructure.Repositories;

public class CommentRepository : JsonFileRepository<Comment>
{
    public CommentRepository() 
        : base("comments.json")
    {
    }

    protected override string GetEntityId(Comment entity) => entity.Id.ToString();
    
    public async Task<IEnumerable<Comment>> GetByEntityAsync(string entityType, string entityId)
    {
        var all = await GetAllAsync();
        return all.Where(c => c.EntityType == entityType && c.EntityId == entityId)
                  .OrderByDescending(c => c.CreatedAt);
    }
}
