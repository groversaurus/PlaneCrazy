# PlaneCrazy

A repository to build an app to handle ADS-B data, and learn some new architectural patterns.

## Project Structure

```
PlaneCrazy/
├── src/
│   └── PlaneCrazy.Core/           # Core library
│       ├── Models/                # Data models
│       ├── Repositories/          # Data access repositories
│       └── Events/                # Event argument classes
├── tests/
│   └── PlaneCrazy.Core.Tests/     # Unit tests
└── examples/
    └── PlaneCrazy.Demo/           # Demo console application
```

## Features

### FavouritesRepository

The `FavouritesRepository` class provides functionality to manage aircraft favourites with persistence and event notifications.

#### Key Features:
- **JSON Persistence**: Loads and saves favourites to a `favourites.json` file
- **Event Emission**: Emits events for favourite lifecycle changes
- **CRUD Operations**: Add, remove, and retrieve favourites
- **Error Handling**: Gracefully handles missing or invalid JSON files

#### Events:
- `FavouriteAdded`: Fired when a favourite is added
- `FavouriteRemoved`: Fired when a favourite is removed
- `FavouritesLoaded`: Fired when favourites are loaded from file
- `FavouritesSaved`: Fired when favourites are saved to file

#### Usage Example:

```csharp
using PlaneCrazy.Core.Models;
using PlaneCrazy.Core.Repositories;

// Create repository
var repository = new FavouritesRepository("favourites.json");

// Subscribe to events
repository.FavouriteAdded += (sender, e) =>
{
    Console.WriteLine($"Added: {e.FavouriteName}");
};

// Load existing favourites
await repository.LoadAsync();

// Add a favourite
repository.Add(new Favourite
{
    Id = "AA123",
    Name = "American Airlines Flight 123",
    AddedAt = DateTime.UtcNow
});

// Save to file
await repository.SaveAsync();

// Get all favourites
var favourites = repository.GetAll();
```

## Building and Testing

### Build the solution:
```bash
dotnet build
```

### Run the tests:
```bash
dotnet test
```

### Run the demo:
```bash
dotnet run --project examples/PlaneCrazy.Demo/PlaneCrazy.Demo.csproj
```

## Requirements

- .NET 10.0 or later
