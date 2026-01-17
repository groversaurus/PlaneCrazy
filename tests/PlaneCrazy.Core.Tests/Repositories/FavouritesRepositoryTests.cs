using PlaneCrazy.Core.Events;
using PlaneCrazy.Core.Models;
using PlaneCrazy.Core.Repositories;
using Xunit;

namespace PlaneCrazy.Core.Tests.Repositories;

public class FavouritesRepositoryTests : IDisposable
{
    private readonly string _testFilePath;
    private readonly FavouritesRepository _repository;

    public FavouritesRepositoryTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test_favourites_{Guid.NewGuid()}.json");
        _repository = new FavouritesRepository(_testFilePath);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Fact]
    public void Constructor_InitializesWithDefaultFilePath()
    {
        var repo = new FavouritesRepository();
        Assert.NotNull(repo);
    }

    [Fact]
    public void GetAll_ReturnsEmptyList_WhenNoFavouritesAdded()
    {
        var favourites = _repository.GetAll();
        Assert.Empty(favourites);
    }

    [Fact]
    public void Add_AddsFavouriteToCollection()
    {
        var favourite = new Favourite
        {
            Id = "1",
            Name = "Test Favourite",
            AddedAt = DateTime.UtcNow
        };

        _repository.Add(favourite);

        var favourites = _repository.GetAll();
        Assert.Single(favourites);
        Assert.Equal("1", favourites[0].Id);
        Assert.Equal("Test Favourite", favourites[0].Name);
    }

    [Fact]
    public void Add_ThrowsArgumentNullException_WhenFavouriteIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _repository.Add(null!));
    }

    [Fact]
    public void Add_RaisesFavouriteAddedEvent()
    {
        var eventRaised = false;
        FavouriteEventArgs? capturedArgs = null;

        _repository.FavouriteAdded += (sender, args) =>
        {
            eventRaised = true;
            capturedArgs = args;
        };

        var favourite = new Favourite
        {
            Id = "1",
            Name = "Test Favourite",
            AddedAt = DateTime.UtcNow
        };

        _repository.Add(favourite);

        Assert.True(eventRaised);
        Assert.NotNull(capturedArgs);
        Assert.Equal("1", capturedArgs.FavouriteId);
        Assert.Equal("Test Favourite", capturedArgs.FavouriteName);
    }

    [Fact]
    public void Remove_RemovesFavouriteById()
    {
        var favourite = new Favourite
        {
            Id = "1",
            Name = "Test Favourite",
            AddedAt = DateTime.UtcNow
        };

        _repository.Add(favourite);
        var result = _repository.Remove("1");

        Assert.True(result);
        Assert.Empty(_repository.GetAll());
    }

    [Fact]
    public void Remove_ReturnsFalse_WhenFavouriteNotFound()
    {
        var result = _repository.Remove("nonexistent");
        Assert.False(result);
    }

    [Fact]
    public void Remove_RaisesFavouriteRemovedEvent()
    {
        var eventRaised = false;
        FavouriteEventArgs? capturedArgs = null;

        var favourite = new Favourite
        {
            Id = "1",
            Name = "Test Favourite",
            AddedAt = DateTime.UtcNow
        };

        _repository.Add(favourite);

        _repository.FavouriteRemoved += (sender, args) =>
        {
            eventRaised = true;
            capturedArgs = args;
        };

        _repository.Remove("1");

        Assert.True(eventRaised);
        Assert.NotNull(capturedArgs);
        Assert.Equal("1", capturedArgs.FavouriteId);
        Assert.Equal("Test Favourite", capturedArgs.FavouriteName);
    }

    [Fact]
    public async Task SaveAsync_CreatesFavouritesJsonFile()
    {
        var favourite = new Favourite
        {
            Id = "1",
            Name = "Test Favourite",
            AddedAt = DateTime.UtcNow
        };

        _repository.Add(favourite);
        await _repository.SaveAsync();

        Assert.True(File.Exists(_testFilePath));
    }

    [Fact]
    public async Task SaveAsync_RaisesFavouritesSavedEvent()
    {
        var eventRaised = false;

        _repository.FavouritesSaved += (sender, args) =>
        {
            eventRaised = true;
        };

        await _repository.SaveAsync();

        Assert.True(eventRaised);
    }

    [Fact]
    public async Task LoadAsync_LoadsFavouritesFromFile()
    {
        var favourite = new Favourite
        {
            Id = "1",
            Name = "Test Favourite",
            AddedAt = DateTime.UtcNow
        };

        _repository.Add(favourite);
        await _repository.SaveAsync();

        var newRepository = new FavouritesRepository(_testFilePath);
        await newRepository.LoadAsync();

        var favourites = newRepository.GetAll();
        Assert.Single(favourites);
        Assert.Equal("1", favourites[0].Id);
        Assert.Equal("Test Favourite", favourites[0].Name);
    }

    [Fact]
    public async Task LoadAsync_RaisesFavouritesLoadedEvent()
    {
        var eventRaised = false;

        _repository.FavouritesLoaded += (sender, args) =>
        {
            eventRaised = true;
        };

        await _repository.LoadAsync();

        Assert.True(eventRaised);
    }

    [Fact]
    public async Task LoadAsync_InitializesEmptyList_WhenFileDoesNotExist()
    {
        await _repository.LoadAsync();

        var favourites = _repository.GetAll();
        Assert.Empty(favourites);
    }

    [Fact]
    public async Task LoadAsync_HandlesInvalidJson_ByInitializingEmptyList()
    {
        await File.WriteAllTextAsync(_testFilePath, "invalid json content");

        await _repository.LoadAsync();

        var favourites = _repository.GetAll();
        Assert.Empty(favourites);
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectoryIfNotExists()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), $"test_dir_{Guid.NewGuid()}");
        var filePath = Path.Combine(directoryPath, "favourites.json");

        try
        {
            var repo = new FavouritesRepository(filePath);
            await repo.SaveAsync();

            Assert.True(Directory.Exists(directoryPath));
            Assert.True(File.Exists(filePath));
        }
        finally
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }
    }

    [Fact]
    public async Task SaveAndLoad_PreservesMultipleFavourites()
    {
        var favourite1 = new Favourite
        {
            Id = "1",
            Name = "First Favourite",
            AddedAt = DateTime.UtcNow
        };

        var favourite2 = new Favourite
        {
            Id = "2",
            Name = "Second Favourite",
            AddedAt = DateTime.UtcNow.AddMinutes(1)
        };

        _repository.Add(favourite1);
        _repository.Add(favourite2);
        await _repository.SaveAsync();

        var newRepository = new FavouritesRepository(_testFilePath);
        await newRepository.LoadAsync();

        var favourites = newRepository.GetAll();
        Assert.Equal(2, favourites.Count);
        Assert.Contains(favourites, f => f.Id == "1" && f.Name == "First Favourite");
        Assert.Contains(favourites, f => f.Id == "2" && f.Name == "Second Favourite");
    }
}
