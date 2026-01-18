using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Tests.Helpers;

/// <summary>
/// In-memory implementation of comment repository for testing.
/// </summary>
public class InMemoryCommentRepository
{
    private readonly Dictionary<string, Comment> _comments = new();
    private readonly object _lock = new();

    public Task<Comment?> GetByIdAsync(string id)
    {
        lock (_lock)
        {
            _comments.TryGetValue(id, out var comment);
            return Task.FromResult(comment);
        }
    }

    public Task<IEnumerable<Comment>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Comment>>(_comments.Values.ToList());
        }
    }

    public Task<IEnumerable<Comment>> GetByEntityAsync(string entityType, string entityId)
    {
        lock (_lock)
        {
            var results = _comments.Values
                .Where(c => c.EntityType == entityType && c.EntityId == entityId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
            return Task.FromResult<IEnumerable<Comment>>(results);
        }
    }

    public Task<IEnumerable<Comment>> GetActiveByEntityAsync(string entityType, string entityId)
    {
        lock (_lock)
        {
            var results = _comments.Values
                .Where(c => c.EntityType == entityType && c.EntityId == entityId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
            return Task.FromResult<IEnumerable<Comment>>(results);
        }
    }

    public Task SaveAsync(Comment entity)
    {
        lock (_lock)
        {
            _comments[entity.Id.ToString()] = entity;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        lock (_lock)
        {
            _comments.Remove(id);
        }
        return Task.CompletedTask;
    }

    public void Clear()
    {
        lock (_lock)
        {
            _comments.Clear();
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _comments.Count;
            }
        }
    }
}
