# PlaneCrazy Query Services Documentation

## Overview

PlaneCrazy implements the **Query** side of CQRS (Command Query Responsibility Segregation). Query services provide optimized read operations against denormalized read models (projections) rather than querying the event store directly.

## Query Architecture

```
User Request
    ↓
Query Service Interface (ICommentQueryService, etc.)
    ↓
Query Service Implementation
    ↓
Repository (reads from projection)
    ↓
Query Result DTO
    ↓
Return to Caller
```

## Core Principles

1. **Read-Only**: Query services never modify state
2. **Projection-Based**: Read from denormalized projections, not event store
3. **DTO Returns**: Return Data Transfer Objects optimized for display
4. **No Business Logic**: Simple data retrieval and mapping
5. **Performance Optimized**: Projections are pre-computed for fast queries

## Query Services

### IAircraftQueryService

**Purpose**: Query operations for aircraft data

**Location**: `PlaneCrazy.Domain/Interfaces/IAircraftQueryService.cs`

**Implementation**: `PlaneCrazy.Infrastructure/QueryServices/AircraftQueryService.cs`

#### Methods

```csharp
public interface IAircraftQueryService
{
    /// <summary>
    /// Gets an aircraft by ICAO24 identifier.
    /// </summary>
    Task<AircraftQueryResult?> GetAircraftByIcaoAsync(string icao24);
    
    /// <summary>
    /// Gets all aircraft currently in the repository.
    /// </summary>
    Task<IEnumerable<AircraftQueryResult>> GetAllAircraftAsync();
    
    /// <summary>
    /// Gets aircraft that match a specific registration.
    /// </summary>
    Task<IEnumerable<AircraftQueryResult>> GetAircraftByRegistrationAsync(string registration);
    
    /// <summary>
    /// Gets aircraft of a specific type.
    /// </summary>
    Task<IEnumerable<AircraftQueryResult>> GetAircraftByTypeAsync(string typeCode);
    
    /// <summary>
    /// Gets recently seen aircraft (within specified minutes).
    /// </summary>
    Task<IEnumerable<AircraftQueryResult>> GetRecentlySeenAircraftAsync(int withinMinutes = 5);
}
```

#### Usage Examples

```csharp
// Get specific aircraft
var aircraft = await aircraftQueryService.GetAircraftByIcaoAsync("A1B2C3");
if (aircraft != null)
{
    Console.WriteLine($"{aircraft.Icao24} - {aircraft.Registration}");
}

// Get all aircraft of a specific type
var boeing737s = await aircraftQueryService.GetAircraftByTypeAsync("B738");
foreach (var plane in boeing737s)
{
    Console.WriteLine($"{plane.Icao24}: {plane.Registration}");
}

// Get recently active aircraft
var recentAircraft = await aircraftQueryService.GetRecentlySeenAircraftAsync(10);
Console.WriteLine($"Found {recentAircraft.Count()} aircraft in last 10 minutes");
```

### ICommentQueryService

**Purpose**: Query operations for comments

**Location**: `PlaneCrazy.Domain/Interfaces/ICommentQueryService.cs`

**Implementation**: `PlaneCrazy.Infrastructure/QueryServices/CommentQueryService.cs`

#### Methods

```csharp
public interface ICommentQueryService
{
    /// <summary>
    /// Gets all comments for a specific entity (including deleted).
    /// </summary>
    Task<IEnumerable<CommentQueryResult>> GetCommentsForEntityAsync(
        string entityType, 
        string entityId);
    
    /// <summary>
    /// Gets only active (non-deleted) comments for an entity.
    /// </summary>
    Task<IEnumerable<CommentQueryResult>> GetActiveCommentsForEntityAsync(
        string entityType, 
        string entityId);
    
    /// <summary>
    /// Gets a single comment by ID.
    /// </summary>
    Task<CommentQueryResult?> GetCommentByIdAsync(Guid commentId);
    
    /// <summary>
    /// Gets all entities that have comments.
    /// Returns tuples of (EntityType, EntityId, CommentCount).
    /// </summary>
    Task<IEnumerable<(string EntityType, string EntityId, int CommentCount)>> 
        GetEntitiesWithCommentsAsync();
    
    /// <summary>
    /// Gets comment count for a specific entity.
    /// </summary>
    Task<int> GetCommentCountAsync(string entityType, string entityId);
    
    /// <summary>
    /// Gets recent comments across all entities.
    /// </summary>
    Task<IEnumerable<CommentQueryResult>> GetRecentCommentsAsync(int count = 10);
}
```

#### Usage Examples

```csharp
// Get active comments for an aircraft
var comments = await commentQueryService
    .GetActiveCommentsForEntityAsync("Aircraft", "A1B2C3");

foreach (var comment in comments)
{
    Console.WriteLine($"[{comment.CreatedAt}] {comment.CreatedBy}: {comment.Text}");
    if (comment.UpdatedAt.HasValue)
    {
        Console.WriteLine($"  (edited {comment.UpdatedAt})");
    }
}

// Get comment count
var count = await commentQueryService
    .GetCommentCountAsync("Aircraft", "A1B2C3");
Console.WriteLine($"Total comments: {count}");

// Get all entities with comments
var entitiesWithComments = await commentQueryService
    .GetEntitiesWithCommentsAsync();

foreach (var (entityType, entityId, commentCount) in entitiesWithComments)
{
    Console.WriteLine($"{entityType} {entityId}: {commentCount} comments");
}

// Get recent comments across all entities
var recentComments = await commentQueryService.GetRecentCommentsAsync(20);
foreach (var comment in recentComments)
{
    Console.WriteLine($"{comment.EntityType}/{comment.EntityId}: {comment.Text}");
}
```

### IFavouriteQueryService

**Purpose**: Query operations for favourites

**Location**: `PlaneCrazy.Domain/Interfaces/IFavouriteQueryService.cs`

**Implementation**: `PlaneCrazy.Infrastructure/QueryServices/FavouriteQueryService.cs`

#### Methods

```csharp
public interface IFavouriteQueryService
{
    /// <summary>
    /// Gets all favourites of a specific type (Aircraft, Type, or Airport).
    /// </summary>
    Task<IEnumerable<FavouriteQueryResult>> GetFavouritesByTypeAsync(
        string entityType);
    
    /// <summary>
    /// Checks if a specific entity is favourited.
    /// </summary>
    Task<bool> IsFavouritedAsync(string entityType, string entityId);
    
    /// <summary>
    /// Gets a specific favourite by entity type and ID.
    /// </summary>
    Task<FavouriteQueryResult?> GetFavouriteAsync(
        string entityType, 
        string entityId);
    
    /// <summary>
    /// Gets all favourites across all entity types.
    /// </summary>
    Task<IEnumerable<FavouriteQueryResult>> GetAllFavouritesAsync();
    
    /// <summary>
    /// Gets favourite aircraft with their comment counts.
    /// </summary>
    Task<IEnumerable<FavouriteWithCommentsResult>> 
        GetFavouriteAircraftWithCommentsAsync();
}
```

#### Usage Examples

```csharp
// Check if aircraft is favourited
bool isFavourite = await favouriteQueryService
    .IsFavouritedAsync("Aircraft", "A1B2C3");

if (isFavourite)
{
    Console.WriteLine("This aircraft is in your favourites!");
}

// Get all favourite aircraft
var favouriteAircraft = await favouriteQueryService
    .GetFavouritesByTypeAsync("Aircraft");

foreach (var fav in favouriteAircraft)
{
    Console.WriteLine($"{fav.EntityId} - Added: {fav.FavouritedAt}");
    Console.WriteLine($"  Registration: {fav.Metadata["Registration"]}");
    Console.WriteLine($"  Type: {fav.Metadata["TypeCode"]}");
}

// Get favourite types
var favouriteTypes = await favouriteQueryService
    .GetFavouritesByTypeAsync("Type");

foreach (var fav in favouriteTypes)
{
    Console.WriteLine($"{fav.EntityId}: {fav.Metadata["TypeName"]}");
}

// Get favourite airports
var favouriteAirports = await favouriteQueryService
    .GetFavouritesByTypeAsync("Airport");

foreach (var fav in favouriteAirports)
{
    Console.WriteLine($"{fav.EntityId}: {fav.Metadata["Name"]}");
}
```

## Query Result DTOs

### AircraftQueryResult

```csharp
public class AircraftQueryResult
{
    public required string Icao24 { get; init; }
    public string? Registration { get; init; }
    public string? TypeCode { get; init; }
    public string? Callsign { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? Altitude { get; init; }
    public double? Velocity { get; init; }
    public double? Track { get; init; }
    public double? VerticalRate { get; init; }
    public bool? OnGround { get; init; }
    public DateTime FirstSeen { get; init; }
    public DateTime LastSeen { get; init; }
    public DateTime? LastUpdated { get; init; }
    public int TotalUpdates { get; init; }
}
```

### CommentQueryResult

```csharp
public class CommentQueryResult
{
    public Guid Id { get; init; }
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public required string Text { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? UpdatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsDeleted { get; init; }
}
```

### FavouriteQueryResult

```csharp
public class FavouriteQueryResult
{
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public DateTime FavouritedAt { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
    public int CommentCount { get; init; }
}
```

### FavouriteWithCommentsResult

```csharp
public class FavouriteWithCommentsResult : FavouriteQueryResult
{
    public List<CommentQueryResult> RecentComments { get; init; } = new();
}
```

## Query Performance

### Optimization Strategies

1. **Pre-computed Projections**: All queries read from projections, not events
2. **In-Memory Caching**: Repositories use singleton lifetime with in-memory state
3. **Indexed Lookups**: Dictionary-based lookups for O(1) access by ID
4. **Filtered Results**: Services filter at the service layer, not in UI

### Performance Characteristics

| Operation | Complexity | Notes |
|-----------|------------|-------|
| Get by ID | O(1) | Dictionary lookup |
| Get all | O(n) | Full scan required |
| Filter by property | O(n) | LINQ filter over collection |
| Get with comments | O(n + m) | Join favourites with comments |

### When to Rebuild Projections

- **Startup**: Application rebuilds projections on startup
- **Data corruption**: If projection files become corrupted
- **Schema changes**: After modifying projection structure
- **Manual trigger**: Via admin command or API

## Query Patterns

### Pattern 1: Simple Lookup

```csharp
// Single entity by ID
var aircraft = await aircraftQueryService.GetAircraftByIcaoAsync("A1B2C3");
```

**Use Case**: Display details for a specific entity

**Performance**: O(1) - Direct dictionary lookup

### Pattern 2: Filter and Project

```csharp
// Get aircraft of specific type, map to display model
var boeing737s = await aircraftQueryService.GetAircraftByTypeAsync("B738");
var displayModels = boeing737s.Select(a => new 
{
    Id = a.Icao24,
    Reg = a.Registration ?? "Unknown",
    LastSeen = a.LastSeen.ToString("HH:mm:ss")
});
```

**Use Case**: List views with filtering

**Performance**: O(n) - Full scan with filter

### Pattern 3: Aggregate Queries

```csharp
// Get entities with comment counts
var entitiesWithComments = await commentQueryService
    .GetEntitiesWithCommentsAsync();

// Group by entity type
var grouped = entitiesWithComments
    .GroupBy(e => e.EntityType)
    .Select(g => new 
    {
        Type = g.Key,
        Count = g.Count(),
        TotalComments = g.Sum(e => e.CommentCount)
    });
```

**Use Case**: Statistics and reporting

**Performance**: O(n) - Full scan with grouping

### Pattern 4: Enriched Results

```csharp
// Get favourites with their comments
var favourites = await favouriteQueryService
    .GetFavouritesByTypeAsync("Aircraft");

foreach (var fav in favourites)
{
    var comments = await commentQueryService
        .GetActiveCommentsForEntityAsync("Aircraft", fav.EntityId);
    
    Console.WriteLine($"{fav.EntityId}: {comments.Count()} comments");
}
```

**Use Case**: Display with related data

**Performance**: O(n * m) - N favourites, M comments each

**Optimization**: Use `GetFavouriteAircraftWithCommentsAsync()` for batched retrieval

## Caching Strategy

### Current Implementation

- **Singleton Repositories**: Services and repositories are singleton, providing implicit caching
- **In-Memory State**: Repositories load data once and cache in memory
- **Write-Through**: Updates write to file and update in-memory cache
- **Manual Rebuild**: Projections rebuilt on startup or manual trigger

### Future Enhancements

1. **Distributed Caching**: Redis for multi-instance deployments
2. **Cache Invalidation**: Selective invalidation on specific updates
3. **TTL Policies**: Time-based expiration for volatile data
4. **Cache Warming**: Pre-load frequently accessed data

## Error Handling

### Query Service Error Patterns

```csharp
// Return null for not found
var aircraft = await aircraftQueryService.GetAircraftByIcaoAsync("NOTFOUND");
// Returns: null

// Return empty collection for no results
var comments = await commentQueryService
    .GetActiveCommentsForEntityAsync("Aircraft", "NOCOMMENTS");
// Returns: Empty IEnumerable<CommentQueryResult>

// Throw exception for invalid input
try
{
    var comments = await commentQueryService
        .GetCommentsForEntityAsync("InvalidType", "ABC123");
}
catch (ArgumentException ex)
{
    // Handle invalid entity type
}
```

### Best Practices

1. **Null-safe**: Always check for null when querying by ID
2. **Empty Collections**: Use `.Any()` to check for results
3. **Validate Input**: Services validate entity types and IDs
4. **Log Queries**: Log slow or failed queries for debugging

## Query Service Implementation Example

### AircraftQueryService Implementation

```csharp
public class AircraftQueryService : IAircraftQueryService
{
    private readonly AircraftRepository _repository;
    
    public AircraftQueryService(AircraftRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<AircraftQueryResult?> GetAircraftByIcaoAsync(string icao24)
    {
        var aircraft = await _repository.GetByIdAsync(icao24);
        return aircraft == null ? null : MapToQueryResult(aircraft);
    }
    
    public async Task<IEnumerable<AircraftQueryResult>> GetAllAircraftAsync()
    {
        var aircraft = await _repository.GetAllAsync();
        return aircraft.Select(MapToQueryResult);
    }
    
    public async Task<IEnumerable<AircraftQueryResult>> GetRecentlySeenAircraftAsync(
        int withinMinutes = 5)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-withinMinutes);
        var aircraft = await _repository.GetAllAsync();
        
        return aircraft
            .Where(a => a.LastSeen >= cutoff)
            .OrderByDescending(a => a.LastSeen)
            .Select(MapToQueryResult);
    }
    
    private AircraftQueryResult MapToQueryResult(Aircraft aircraft)
    {
        return new AircraftQueryResult
        {
            Icao24 = aircraft.Icao24,
            Registration = aircraft.Registration,
            TypeCode = aircraft.TypeCode,
            Callsign = aircraft.Callsign,
            Latitude = aircraft.Latitude,
            Longitude = aircraft.Longitude,
            Altitude = aircraft.Altitude,
            Velocity = aircraft.Velocity,
            Track = aircraft.Track,
            VerticalRate = aircraft.VerticalRate,
            OnGround = aircraft.OnGround,
            FirstSeen = aircraft.FirstSeen,
            LastSeen = aircraft.LastSeen,
            LastUpdated = aircraft.LastUpdated,
            TotalUpdates = aircraft.TotalUpdates
        };
    }
}
```

## Testing Query Services

### Unit Test Example

```csharp
[Test]
public async Task GetActiveCommentsForEntityAsync_FiltersDeletedComments()
{
    // Arrange
    var repository = new InMemoryCommentRepository();
    await repository.SaveAsync(new Comment 
    { 
        Id = Guid.NewGuid(), 
        EntityType = "Aircraft",
        EntityId = "ABC123",
        Text = "Active comment",
        IsDeleted = false 
    });
    await repository.SaveAsync(new Comment 
    { 
        Id = Guid.NewGuid(), 
        EntityType = "Aircraft",
        EntityId = "ABC123",
        Text = "Deleted comment",
        IsDeleted = true 
    });
    
    var queryService = new CommentQueryService(repository);
    
    // Act
    var results = await queryService
        .GetActiveCommentsForEntityAsync("Aircraft", "ABC123");
    
    // Assert
    Assert.That(results.Count(), Is.EqualTo(1));
    Assert.That(results.First().Text, Is.EqualTo("Active comment"));
}
```

## Common Query Scenarios

### Scenario 1: Display Aircraft Details

```csharp
public async Task DisplayAircraftDetails(string icao24)
{
    var aircraft = await _aircraftQueryService.GetAircraftByIcaoAsync(icao24);
    if (aircraft == null)
    {
        Console.WriteLine("Aircraft not found");
        return;
    }
    
    Console.WriteLine($"ICAO24: {aircraft.Icao24}");
    Console.WriteLine($"Registration: {aircraft.Registration ?? "Unknown"}");
    Console.WriteLine($"Type: {aircraft.TypeCode ?? "Unknown"}");
    Console.WriteLine($"Last Seen: {aircraft.LastSeen}");
    
    // Check if favourited
    bool isFav = await _favouriteQueryService
        .IsFavouritedAsync("Aircraft", icao24);
    if (isFav)
    {
        Console.WriteLine("★ Favourited");
    }
    
    // Show comments
    var comments = await _commentQueryService
        .GetActiveCommentsForEntityAsync("Aircraft", icao24);
    Console.WriteLine($"\nComments ({comments.Count()}):");
    foreach (var comment in comments)
    {
        Console.WriteLine($"  [{comment.CreatedAt}] {comment.Text}");
    }
}
```

### Scenario 2: List Favourite Aircraft with Status

```csharp
public async Task ListFavouriteAircraftWithStatus()
{
    var favourites = await _favouriteQueryService
        .GetFavouritesByTypeAsync("Aircraft");
    
    foreach (var fav in favourites)
    {
        var aircraft = await _aircraftQueryService
            .GetAircraftByIcaoAsync(fav.EntityId);
        
        var status = aircraft != null && 
            aircraft.LastSeen > DateTime.UtcNow.AddMinutes(-5)
            ? "🟢 Active" 
            : "⚫ Inactive";
        
        Console.WriteLine($"{fav.EntityId} - {status}");
        Console.WriteLine($"  Registration: {fav.Metadata["Registration"]}");
        Console.WriteLine($"  Added: {fav.FavouritedAt:yyyy-MM-dd}");
    }
}
```

### Scenario 3: Search and Filter

```csharp
public async Task SearchAircraftByRegistration(string searchTerm)
{
    var allAircraft = await _aircraftQueryService.GetAllAircraftAsync();
    
    var results = allAircraft
        .Where(a => a.Registration?.Contains(searchTerm, 
            StringComparison.OrdinalIgnoreCase) == true)
        .OrderBy(a => a.Registration);
    
    foreach (var aircraft in results)
    {
        Console.WriteLine($"{aircraft.Registration} ({aircraft.Icao24})");
    }
}
```

## Best Practices

1. **Always use Query Services**: Don't query repositories directly from UI
2. **Return DTOs**: Never return domain entities from query services
3. **Filter in Service**: Apply filters in the service layer, not UI
4. **Async All The Way**: Use async/await throughout the query chain
5. **Null Safety**: Always handle null results from queries
6. **Pagination**: For large result sets, implement pagination
7. **Logging**: Log slow queries for performance monitoring
8. **Validation**: Validate query parameters before executing

## Future Enhancements

1. **Pagination Support**: Add skip/take parameters for large result sets
2. **Sorting Options**: Configurable sort order for collections
3. **Advanced Filtering**: Complex filter expressions
4. **Aggregation Queries**: Count, sum, average operations
5. **Full-Text Search**: Search across multiple fields
6. **GraphQL API**: Query language for flexible data retrieval
7. **OData Support**: Standardized query syntax
8. **Real-time Queries**: Subscribe to query result changes

---

*Last Updated: January 24, 2026*
