using System.Text.Json;
using PlaneCrazy.Core.Events;
using PlaneCrazy.Core.Models;

namespace PlaneCrazy.Core.Repositories;

public class FavouritesRepository
{
    private readonly string _filePath;
    private List<Favourite> _favourites;

    public event EventHandler<FavouriteEventArgs>? FavouriteAdded;
    public event EventHandler<FavouriteEventArgs>? FavouriteRemoved;
    public event EventHandler? FavouritesLoaded;
    public event EventHandler? FavouritesSaved;

    public FavouritesRepository(string filePath = "favourites.json")
    {
        _filePath = filePath;
        _favourites = new List<Favourite>();
    }

    public IReadOnlyList<Favourite> GetAll()
    {
        return _favourites.AsReadOnly();
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            _favourites = new List<Favourite>();
            OnFavouritesLoaded();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            _favourites = JsonSerializer.Deserialize<List<Favourite>>(json) ?? new List<Favourite>();
            OnFavouritesLoaded();
        }
        catch (JsonException)
        {
            _favourites = new List<Favourite>();
            OnFavouritesLoaded();
        }
    }

    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(_favourites, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await File.WriteAllTextAsync(_filePath, json);
        OnFavouritesSaved();
    }

    public void Add(Favourite favourite)
    {
        if (favourite == null)
            throw new ArgumentNullException(nameof(favourite));

        _favourites.Add(favourite);
        OnFavouriteAdded(new FavouriteEventArgs 
        { 
            FavouriteId = favourite.Id, 
            FavouriteName = favourite.Name 
        });
    }

    public bool Remove(string id)
    {
        var favourite = _favourites.FirstOrDefault(f => f.Id == id);
        if (favourite == null)
            return false;

        var removed = _favourites.Remove(favourite);
        if (removed)
        {
            OnFavouriteRemoved(new FavouriteEventArgs 
            { 
                FavouriteId = favourite.Id, 
                FavouriteName = favourite.Name 
            });
        }
        return removed;
    }

    protected virtual void OnFavouriteAdded(FavouriteEventArgs e)
    {
        FavouriteAdded?.Invoke(this, e);
    }

    protected virtual void OnFavouriteRemoved(FavouriteEventArgs e)
    {
        FavouriteRemoved?.Invoke(this, e);
    }

    protected virtual void OnFavouritesLoaded()
    {
        FavouritesLoaded?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnFavouritesSaved()
    {
        FavouritesSaved?.Invoke(this, EventArgs.Empty);
    }
}
