using System.Text.Json;
using PlaneCrazy.Domain.Interfaces;

namespace PlaneCrazy.Infrastructure.Repositories;

public abstract class JsonFileRepository<T> : IRepository<T> where T : class
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    protected JsonFileRepository(string basePath, string fileName)
    {
        var repositoryPath = Path.Combine(basePath, "Repositories");
        Directory.CreateDirectory(repositoryPath);
        _filePath = Path.Combine(repositoryPath, fileName);
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    protected abstract string GetEntityId(T entity);

    public async Task<T?> GetByIdAsync(string id)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(e => GetEntityId(e) == id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        if (!File.Exists(_filePath))
        {
            return Enumerable.Empty<T>();
        }

        await _semaphore.WaitAsync();
        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            var items = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions);
            return items ?? Enumerable.Empty<T>();
        }
        catch
        {
            return Enumerable.Empty<T>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SaveAsync(T entity)
    {
        await _semaphore.WaitAsync();
        try
        {
            List<T> all;
            if (!File.Exists(_filePath))
            {
                all = new List<T>();
            }
            else
            {
                var json = await File.ReadAllTextAsync(_filePath);
                all = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
            }
            
            var entityId = GetEntityId(entity);
            var existingIndex = all.FindIndex(e => GetEntityId(e) == entityId);
            
            if (existingIndex >= 0)
            {
                all[existingIndex] = entity;
            }
            else
            {
                all.Add(entity);
            }

            var outputJson = JsonSerializer.Serialize(all, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, outputJson);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task DeleteAsync(string id)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_filePath))
            {
                return;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            var all = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
            
            all.RemoveAll(e => GetEntityId(e) == id);
            
            var outputJson = JsonSerializer.Serialize(all, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, outputJson);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
