using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Tests.Helpers;

/// <summary>
/// In-memory implementation of favourite repository for testing.
/// </summary>
public class InMemoryFavouriteRepository
{
    private readonly Dictionary<string, Favourite> _favourites = new();
    private readonly object _lock = new();

    public Task<Favourite?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            _favourites.TryGetValue(id, out var favourite);
            return Task.FromResult(favourite);
        }
    }

    public Task<IEnumerable<Favourite>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Favourite>>(_favourites.Values.ToList());
        }
    }

    public Task<IEnumerable<Favourite>> GetByEntityTypeAsync(string entityType)
    {
        lock (_lock)
        {
            var results = _favourites.Values
                .Where(f => f.EntityType == entityType)
                .ToList();
            return Task.FromResult<IEnumerable<Favourite>>(results);
        }
    }

    public Task SaveAsync(Favourite entity)
    {
        lock (_lock)
        {
            var id = $"{entity.EntityType}_{entity.EntityId}";
            _favourites[id] = entity;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        lock (_lock)
        {
            _favourites.Remove(id);
        }
        return Task.CompletedTask;
    }

    public void Clear()
    {
        lock (_lock)
        {
            _favourites.Clear();
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _favourites.Count;
            }
        }
    }
}
