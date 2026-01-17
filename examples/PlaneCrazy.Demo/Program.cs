using PlaneCrazy.Core.Events;
using PlaneCrazy.Core.Models;
using PlaneCrazy.Core.Repositories;

Console.WriteLine("=== FavouritesRepository Demo ===\n");

// Create repository
var repository = new FavouritesRepository("demo_favourites.json");

// Subscribe to events
repository.FavouriteAdded += (sender, e) =>
{
    Console.WriteLine($"✓ Favourite Added: {e.FavouriteName} (ID: {e.FavouriteId})");
};

repository.FavouriteRemoved += (sender, e) =>
{
    Console.WriteLine($"✗ Favourite Removed: {e.FavouriteName} (ID: {e.FavouriteId})");
};

repository.FavouritesLoaded += (sender, e) =>
{
    Console.WriteLine("✓ Favourites loaded from file");
};

repository.FavouritesSaved += (sender, e) =>
{
    Console.WriteLine("✓ Favourites saved to file\n");
};

// Load existing favourites
Console.WriteLine("Loading favourites...");
await repository.LoadAsync();

// Add some favourites
Console.WriteLine("\nAdding favourites...");
repository.Add(new Favourite
{
    Id = "AA123",
    Name = "American Airlines Flight 123",
    AddedAt = DateTime.UtcNow
});

repository.Add(new Favourite
{
    Id = "BA456",
    Name = "British Airways Flight 456",
    AddedAt = DateTime.UtcNow
});

// Display all favourites
Console.WriteLine("\nCurrent favourites:");
foreach (var fav in repository.GetAll())
{
    Console.WriteLine($"  - {fav.Name} (ID: {fav.Id}, Added: {fav.AddedAt:yyyy-MM-dd HH:mm:ss})");
}

// Save to file
Console.WriteLine("\nSaving favourites...");
await repository.SaveAsync();

// Remove a favourite
Console.WriteLine("Removing a favourite...");
repository.Remove("AA123");

// Display remaining favourites
Console.WriteLine("\nRemaining favourites:");
foreach (var fav in repository.GetAll())
{
    Console.WriteLine($"  - {fav.Name} (ID: {fav.Id})");
}

// Save changes
Console.WriteLine("\nSaving updated favourites...");
await repository.SaveAsync();

Console.WriteLine("\n=== Demo Complete ===");
Console.WriteLine($"Check 'demo_favourites.json' to see the saved data.");
