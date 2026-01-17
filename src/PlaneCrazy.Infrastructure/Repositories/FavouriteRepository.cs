using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Infrastructure.Repositories;

public class FavouriteRepository : JsonFileRepository<Favourite>
{
    public FavouriteRepository(string basePath) 
        : base(basePath, "favourites.json")
    {
    }

    protected override string GetEntityId(Favourite entity) => $"{entity.EntityType}_{entity.EntityId}";
    
    public async Task<IEnumerable<Favourite>> GetByEntityTypeAsync(string entityType)
    {
        var all = await GetAllAsync();
        return all.Where(f => f.EntityType == entityType);
    }
}
