# PlaneCrazy Projections Documentation

## Overview

Projections are denormalized read models built from domain events. They transform the event stream into optimized data structures for querying. In PlaneCrazy, projections are the foundation of the read side in the CQRS pattern.

## What are Projections?

**Projection** = A read model built by replaying domain events

```
Event Store (Source of Truth)
    ├─→ Event 1: AircraftFavourited
    ├─→ Event 2: CommentAdded  
    ├─→ Event 3: CommentEdited
    └─→ Event 4: AircraftUnfavourited
         ↓
    Projection replays events
         ↓
    Read Model (optimized for queries)
```

## Core Concepts

### Event Sourcing Foundation
- **Events are immutable**: Never modified or deleted
- **Projections are derived**: Can be rebuilt from events at any time
- **Eventual consistency**: Projections updated after event persisted
- **Multiple projections**: Same events can build different read models

### Projection Lifecycle

1. **Initial Build**: Replay all historical events
2. **Real-time Updates**: Apply new events as they occur
3. **Rebuild**: Clear and replay from scratch if needed
4. **Error Recovery**: Failed projection updates can be retried

## IProjection Interface

All projections implement this interface:

```csharp
public interface IProjection
{
    /// <summary>
    /// Applies a single event to update the projection.
    /// </summary>
    /// <param name="domainEvent">The event to apply.</param>
    /// <returns>True if the projection handled the event, false if ignored.</returns>
    Task<bool> ApplyEventAsync(DomainEvent domainEvent);
    
    /// <summary>
    /// Gets the name of this projection for logging/debugging.
    /// </summary>
    string ProjectionName { get; }
}
```

### Key Methods

- **ApplyEventAsync**: Called for each event (historical or new)
- Returns `true` if event was handled, `false` if ignored
- Projections only handle events they care about

## Registered Projections

### 1. FavouriteProjection

**Purpose**: Builds read model of favourited entities

**Location**: `PlaneCrazy.Infrastructure/Projections/FavouriteProjection.cs`

**Events Handled**:
- `AircraftFavourited` → Add aircraft to favourites
- `AircraftUnfavourited` → Remove aircraft from favourites
- `TypeFavourited` → Add type to favourites
- `TypeUnfavourited` → Remove type from favourites
- `AirportFavourited` → Add airport to favourites
- `AirportUnfavourited` → Remove airport from favourites

**Output**: Writes to `FavouriteRepository` → `favourites.json`

**Example Event Handling**:
```csharp
public async Task<bool> ApplyEventAsync(DomainEvent @event)
{
    switch (@event)
    {
        case AircraftFavourited aircraftFavourited:
            await _favouriteRepository.SaveAsync(new Favourite
            {
                EntityType = "Aircraft",
                EntityId = aircraftFavourited.Icao24,
                FavouritedAt = aircraftFavourited.OccurredAt,
                Metadata = new Dictionary<string, string>
                {
                    ["Registration"] = aircraftFavourited.Registration ?? "",
                    ["TypeCode"] = aircraftFavourited.TypeCode ?? ""
                }
            });
            return true;
            
        case AircraftUnfavourited aircraftUnfavourited:
            await _favouriteRepository.DeleteAsync(
                $"Aircraft_{aircraftUnfavourited.Icao24}");
            return true;
            
        default:
            return false; // Event not handled by this projection
    }
}
```

### 2. CommentProjection

**Purpose**: Builds read model of comments on entities

**Location**: `PlaneCrazy.Infrastructure/Projections/CommentProjection.cs`

**Events Handled**:
- `CommentAdded` → Create new comment
- `CommentEdited` → Update comment text
- `CommentDeleted` → Mark comment as deleted (soft delete)

**Output**: Writes to `CommentRepository` → `comments.json`

**State Management**:
```csharp
case CommentAdded commentAdded:
    await _commentRepository.SaveAsync(new Comment
    {
        Id = commentAdded.Id,
        EntityType = commentAdded.EntityType,
        EntityId = commentAdded.EntityId,
        Text = commentAdded.Text,
        CreatedBy = commentAdded.User,
        CreatedAt = commentAdded.OccurredAt,
        IsDeleted = false
    });
    return true;

case CommentEdited commentEdited:
    var comment = await _commentRepository.GetByIdAsync(
        commentEdited.CommentId.ToString());
    if (comment != null)
    {
        comment.Text = commentEdited.Text;
        comment.UpdatedBy = commentEdited.User;
        comment.UpdatedAt = commentEdited.OccurredAt;
        await _commentRepository.SaveAsync(comment);
    }
    return true;

case CommentDeleted commentDeleted:
    var commentToDelete = await _commentRepository.GetByIdAsync(
        commentDeleted.CommentId.ToString());
    if (commentToDelete != null)
    {
        commentToDelete.IsDeleted = true;
        await _commentRepository.SaveAsync(commentToDelete);
    }
    return true;
```

### 3. AircraftStateProjection

**Purpose**: Builds current state of aircraft from position/identity events

**Location**: `PlaneCrazy.Infrastructure/Projections/AircraftStateProjection.cs`

**Events Handled**:
- `AircraftFirstSeen` → Initialize aircraft record
- `AircraftPositionUpdated` → Update position data
- `AircraftIdentityUpdated` → Update registration, type, callsign
- `AircraftLastSeen` → Update last seen timestamp

**Output**: Writes to `AircraftRepository` → `aircraft.json`

**State Reconstruction**:
```csharp
case AircraftFirstSeen firstSeen:
    await _aircraftRepository.SaveAsync(new Aircraft
    {
        Icao24 = firstSeen.Icao24,
        FirstSeen = firstSeen.FirstSeenAt,
        LastSeen = firstSeen.FirstSeenAt,
        Latitude = firstSeen.InitialLatitude,
        Longitude = firstSeen.InitialLongitude
    });
    return true;

case AircraftPositionUpdated positionUpdated:
    var aircraft = await _aircraftRepository.GetByIdAsync(
        positionUpdated.Icao24);
    if (aircraft != null)
    {
        aircraft.Latitude = positionUpdated.Latitude;
        aircraft.Longitude = positionUpdated.Longitude;
        aircraft.Altitude = positionUpdated.Altitude;
        aircraft.Velocity = positionUpdated.Velocity;
        aircraft.Track = positionUpdated.Track;
        aircraft.LastSeen = positionUpdated.Timestamp;
        aircraft.LastUpdated = positionUpdated.OccurredAt;
        aircraft.TotalUpdates++;
        await _aircraftRepository.SaveAsync(aircraft);
    }
    return true;
```

### 4. SnapshotProjection

**Purpose**: Point-in-time aircraft state reconstruction (temporal queries)

**Location**: `PlaneCrazy.Infrastructure/Projections/SnapshotProjection.cs`

**Special Features**:
- Not registered with Event Dispatcher (used manually)
- Rebuilds state at specific timestamp
- Enables "time travel" queries

**Usage**:
```csharp
var snapshot = new SnapshotProjection(eventStore);

// Get aircraft state at specific time
var aircraft = await snapshot.GetAircraftStateAtAsync(
    "A1B2C3", 
    DateTime.Parse("2026-01-24T14:00:00Z"));

// Get all aircraft at specific time
var allAircraft = await snapshot.GetSnapshotAtAsync(
    DateTime.Parse("2026-01-24T14:00:00Z"));

// Get time series data
var series = await snapshot.GetSnapshotSeriesAsync(
    startTime: DateTime.UtcNow.AddHours(-1),
    endTime: DateTime.UtcNow,
    interval: TimeSpan.FromMinutes(10));
```

## Projection Update Flow

### 1. Event Creation

```
User Action → Command → Command Handler → Domain Event
```

### 2. Event Dispatch

```csharp
// Event Dispatcher coordinates the update process
var result = await eventDispatcher.DispatchAsync(domainEvent);

// Internally:
// 1. Validate event
// 2. Write to event store
// 3. Apply to all projections
```

### 3. Projection Update

```csharp
foreach (var projection in _projections)
{
    var handled = await projection.ApplyEventAsync(domainEvent);
    if (handled)
    {
        // Projection updated its read model
        // Changes persisted to repository
    }
}
```

### 4. Persistence

```
Projection → Repository → JSON File
```

## Rebuilding Projections

### When to Rebuild

1. **Application Startup**: Ensures projections are up to date
2. **Data Corruption**: If projection files are corrupted
3. **Schema Changes**: After modifying projection structure
4. **New Projection**: When adding a new projection type
5. **Debugging**: To verify event replay logic

### Manual Rebuild

```csharp
// Rebuild all favourites
await favouriteProjection.RebuildAsync();

// Rebuild all comments
await commentProjection.RebuildAsync();

// Rebuild specific entity's comments
await commentProjection.RebuildForEntityAsync("Aircraft", "A1B2C3");
```

### Rebuild Process

```csharp
public async Task RebuildAsync()
{
    // 1. Clear existing read model
    var allEntities = await _repository.GetAllAsync();
    foreach (var entity in allEntities)
    {
        await _repository.DeleteAsync(entity.Id);
    }
    
    // 2. Get all events from event store
    var events = await _eventStore.GetAllAsync();
    
    // 3. Replay events in chronological order
    foreach (var @event in events)
    {
        await ApplyEventAsync(@event);
    }
}
```

### Startup Rebuild

In `Program.cs`:
```csharp
private static async Task RebuildProjectionsAsync()
{
    Console.WriteLine("Rebuilding projections from event store...");
    await _favouriteProjection.RebuildAsync();
    await _commentProjection.RebuildAsync();
    Console.WriteLine("Projections rebuilt successfully.");
}
```

## Projection Consistency

### Eventual Consistency

Projections are **eventually consistent** with the event store:

1. Event written to event store (immediate)
2. Projections updated (within milliseconds)
3. Query services read from projections (may have slight delay)

### Consistency Guarantees

- **Write-Then-Read**: Reading immediately after write may see old data
- **Sequential Consistency**: Events applied in order they occurred
- **Per-Projection**: Each projection is independently consistent

### Handling Inconsistency

```csharp
// Write operation
await commandHandler.HandleAsync(addCommentCommand);

// Small delay to ensure projection updated (if needed in tests)
await Task.Delay(10);

// Read operation (should see new comment)
var comments = await commentQueryService
    .GetActiveCommentsForEntityAsync("Aircraft", "ABC123");
```

## Error Handling in Projections

### Event Application Errors

```csharp
try
{
    var handled = await projection.ApplyEventAsync(domainEvent);
    projectionResult.Success = true;
    projectionResult.EventHandled = handled;
}
catch (Exception ex)
{
    projectionResult.Success = false;
    projectionResult.Error = ex.Message;
    projectionResult.Exception = ex;
    
    _logger?.LogError(ex, 
        "Projection {Projection} failed to apply event {EventId}",
        projection.ProjectionName, domainEvent.Id);
}
```

### Resilience Strategies

1. **Event Still Persisted**: Even if projection fails, event is in event store
2. **Projection Can Rebuild**: Failed projections can be rebuilt from events
3. **Independent Failures**: One projection failing doesn't affect others
4. **Logged Errors**: All failures logged for investigation

### Recovery Process

```bash
# If projection is corrupted or failed
1. Stop the application
2. Delete corrupted projection file
3. Restart application (auto-rebuilds on startup)
# OR
4. Manually trigger rebuild via API/command
```

## Projection Performance

### Current Performance Characteristics

| Projection | Events Handled | Complexity | Rebuild Time (10k events) |
|------------|----------------|------------|---------------------------|
| FavouriteProjection | 6 types | O(n) | ~1 second |
| CommentProjection | 3 types | O(n) | ~1 second |
| AircraftStateProjection | 4 types | O(n) | ~2 seconds |
| SnapshotProjection | 4 types | O(n*m) | ~5 seconds (on-demand) |

### Optimization Strategies

1. **Selective Rebuilds**: Rebuild only affected entities
2. **Parallel Processing**: Apply events in parallel (if order-independent)
3. **Snapshot Storage**: Cache snapshots to avoid full replay
4. **Incremental Updates**: Only process new events since last update
5. **Batching**: Group repository writes for better I/O performance

### Example: Selective Rebuild

```csharp
public async Task RebuildForEntityAsync(string entityType, string entityId)
{
    // Delete existing comments for this entity
    var existingComments = await _commentRepository.GetAllAsync();
    foreach (var comment in existingComments.Where(c => 
        c.EntityType == entityType && c.EntityId == entityId))
    {
        await _commentRepository.DeleteAsync(comment.Id.ToString());
    }
    
    // Replay only events for this entity
    var events = await _eventStore.GetAllAsync();
    var relevantEvents = events.OfType<CommentEvent>()
        .Where(e => e.EntityType == entityType && e.EntityId == entityId);
    
    foreach (var @event in relevantEvents)
    {
        await ApplyEventAsync(@event);
    }
}
```

## Creating a New Projection

### Step 1: Implement IProjection

```csharp
public class MyCustomProjection : IProjection
{
    private readonly IEventStore _eventStore;
    private readonly MyCustomRepository _repository;
    
    public string ProjectionName => "MyCustomProjection";
    
    public MyCustomProjection(
        IEventStore eventStore, 
        MyCustomRepository repository)
    {
        _eventStore = eventStore;
        _repository = repository;
    }
    
    public async Task<bool> ApplyEventAsync(DomainEvent @event)
    {
        switch (@event)
        {
            case MyCustomEvent myEvent:
                await HandleMyCustomEventAsync(myEvent);
                return true;
            default:
                return false;
        }
    }
    
    private async Task HandleMyCustomEventAsync(MyCustomEvent @event)
    {
        // Update read model based on event
        await _repository.SaveAsync(new MyCustomModel
        {
            // Map event properties to model
        });
    }
    
    public async Task RebuildAsync()
    {
        // Clear existing data
        // Replay all events
    }
}
```

### Step 2: Register in DI Container

```csharp
// In ServiceCollectionExtensions.cs
services.AddSingleton<MyCustomRepository>();
services.AddSingleton<IProjection, MyCustomProjection>();
```

### Step 3: Use in Application

```csharp
// Projection automatically receives all events via Event Dispatcher
// No additional code needed in command handlers

// Query the projection via repository
var data = await myCustomRepository.GetAllAsync();
```

## Projection Design Patterns

### Pattern 1: Additive Projection

Events only add data, never remove:

```csharp
case ItemAdded added:
    await _repository.SaveAsync(new Item { ... });
    return true;
```

**Use Case**: Audit logs, history tracking

### Pattern 2: State Replacement Projection

Events completely replace entity state:

```csharp
case StateUpdated updated:
    var entity = await _repository.GetByIdAsync(updated.Id) 
        ?? new Entity { Id = updated.Id };
    entity.State = updated.NewState;
    await _repository.SaveAsync(entity);
    return true;
```

**Use Case**: Current aircraft position, latest status

### Pattern 3: Aggregate Projection

Events aggregate/summarize data:

```csharp
case MetricRecorded metric:
    var stats = await _repository.GetByIdAsync("daily_stats") 
        ?? new Statistics();
    stats.Count++;
    stats.Total += metric.Value;
    stats.Average = stats.Total / stats.Count;
    await _repository.SaveAsync(stats);
    return true;
```

**Use Case**: Statistics, metrics, summaries

### Pattern 4: Relationship Projection

Events build relationships between entities:

```csharp
case RelationshipCreated created:
    await _repository.SaveAsync(new Relationship
    {
        ParentId = created.ParentId,
        ChildId = created.ChildId,
        Type = created.RelationType
    });
    return true;
```

**Use Case**: Graph data, entity relationships

## Testing Projections

### Unit Test Example

```csharp
[Test]
public async Task ApplyEventAsync_AircraftFavourited_AddsToRepository()
{
    // Arrange
    var eventStore = new InMemoryEventStore();
    var repository = new InMemoryFavouriteRepository();
    var projection = new FavouriteProjection(eventStore, repository);
    
    var @event = new AircraftFavourited
    {
        Icao24 = "ABC123",
        Registration = "N12345",
        TypeCode = "B738"
    };
    
    // Act
    var handled = await projection.ApplyEventAsync(@event);
    
    // Assert
    Assert.That(handled, Is.True);
    var favourites = await repository.GetAllAsync();
    Assert.That(favourites.Count(), Is.EqualTo(1));
    Assert.That(favourites.First().EntityId, Is.EqualTo("ABC123"));
}
```

### Integration Test Example

```csharp
[Test]
public async Task RebuildAsync_ReplaysAllEvents()
{
    // Arrange
    var eventStore = new InMemoryEventStore();
    await eventStore.AppendAsync(new AircraftFavourited { Icao24 = "A1" });
    await eventStore.AppendAsync(new AircraftFavourited { Icao24 = "A2" });
    await eventStore.AppendAsync(new AircraftUnfavourited { Icao24 = "A1" });
    
    var repository = new InMemoryFavouriteRepository();
    var projection = new FavouriteProjection(eventStore, repository);
    
    // Act
    await projection.RebuildAsync();
    
    // Assert
    var favourites = await repository.GetAllAsync();
    Assert.That(favourites.Count(), Is.EqualTo(1));
    Assert.That(favourites.First().EntityId, Is.EqualTo("A2"));
}
```

## Monitoring Projections

### Projection Statistics

```csharp
var stats = eventDispatcher.GetProjectionStatistics();
Console.WriteLine($"Total Projections: {stats.TotalProjections}");
foreach (var name in stats.ProjectionNames)
{
    Console.WriteLine($"  - {name}");
}
```

### Event Dispatch Results

```csharp
var result = await eventDispatcher.DispatchAsync(domainEvent);
Console.WriteLine($"Event: {result.EventType}");
Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"Projections Updated: {result.ProjectionsUpdated}");
Console.WriteLine($"Total Time: {result.TotalTimeMs}ms");

foreach (var projResult in result.ProjectionResults)
{
    Console.WriteLine($"  {projResult.ProjectionName}: " +
        $"{(projResult.Success ? "✓" : "✗")} " +
        $"({projResult.UpdateTimeMs}ms)");
}
```

## Best Practices

1. **Idempotent**: Applying same event multiple times should produce same result
2. **Fast Updates**: Keep projection logic simple and fast
3. **Error Handling**: Gracefully handle missing data or corrupt events
4. **Logging**: Log all projection updates for debugging
5. **Versioning**: Plan for event schema evolution
6. **Testing**: Test both event application and full rebuilds
7. **Monitoring**: Track projection update performance
8. **Documentation**: Document what events each projection handles

## Common Issues and Solutions

### Issue: Projection Out of Sync

**Symptom**: Query results don't match expected state

**Solution**:
```csharp
// Rebuild projection from scratch
await projection.RebuildAsync();
```

### Issue: Slow Rebuild

**Symptom**: Application takes long time to start

**Solution**:
- Implement selective rebuilds
- Use snapshot-based rebuilds
- Optimize repository batch writes
- Consider background rebuild

### Issue: Projection Update Failure

**Symptom**: Events persisted but projection not updated

**Solution**:
- Check logs for error details
- Verify event handling logic
- Ensure repository is accessible
- Rebuild projection if corrupted

## Future Enhancements

1. **Async Projections**: Update projections asynchronously via message queue
2. **Versioned Projections**: Support multiple projection versions
3. **Snapshot Support**: Cache intermediate states for faster rebuilds
4. **Projection Health**: Health check endpoints for monitoring
5. **Custom Rebuild Strategies**: Different rebuild approaches per projection
6. **Event Filtering**: Subscribe to specific event types only
7. **Distributed Projections**: Run projections on separate processes
8. **Real-time Updates**: Push projection updates to connected clients

---

*Last Updated: January 24, 2026*
