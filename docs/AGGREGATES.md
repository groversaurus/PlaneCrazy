# PlaneCrazy Aggregates Documentation

## Overview

This document provides an in-depth exploration of the Aggregate pattern as implemented in PlaneCrazy. Aggregates are the cornerstone of our domain-driven design (DDD) and event sourcing architecture.

## What is an Aggregate?

An **Aggregate** is a cluster of domain objects that can be treated as a single unit for data changes. It consists of:

1. **Aggregate Root**: The primary entity that controls access to the aggregate
2. **Entities**: Objects within the aggregate boundary
3. **Value Objects**: Immutable objects that describe characteristics
4. **Invariants**: Business rules that must always be true

### Key Principles

1. **Single Entry Point**: All modifications go through the aggregate root
2. **Consistency Boundary**: Changes within an aggregate are atomic
3. **Identity**: Each aggregate has a unique identifier
4. **Encapsulation**: Internal state is private; changes via methods only
5. **Event-Based State**: State changes emit domain events

## Base Aggregate Implementation

### AggregateRoot Abstract Class

**Location**: [PlaneCrazy.Domain/Aggregates/AggregateRoot.cs](../src/PlaneCrazy.Domain/Aggregates/AggregateRoot.cs)

```csharp
public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _uncommittedEvents = new();
    
    public abstract string Id { get; }
    public int Version { get; protected set; }
    
    // Collect uncommitted events
    public IEnumerable<DomainEvent> GetUncommittedEvents()
    {
        return _uncommittedEvents.AsReadOnly();
    }
    
    // Mark events as committed
    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }
    
    // Add event to uncommitted list
    protected void AddEvent(DomainEvent @event)
    {
        _uncommittedEvents.Add(@event);
        Apply(@event);
        Version++;
    }
    
    // Load aggregate from historical events
    public void LoadFromHistory(IEnumerable<DomainEvent> events)
    {
        foreach (var @event in events)
        {
            Apply(@event);
            Version++;
        }
    }
    
    // Apply event to update internal state
    protected abstract void Apply(DomainEvent @event);
}
```

### Key Methods

**`AddEvent(DomainEvent @event)`**:
- Adds event to uncommitted event list
- Applies event to update internal state
- Increments version number
- Called when command creates new state changes

**`LoadFromHistory(IEnumerable<DomainEvent> events)`**:
- Replays historical events to rebuild state
- Does NOT add to uncommitted events
- Used when loading aggregate from event store
- Enables event sourcing pattern

**`Apply(DomainEvent @event)`**:
- Abstract method implemented by concrete aggregates
- Updates internal state based on event
- Should NOT have side effects
- Should be idempotent

**`GetUncommittedEvents()`**:
- Returns events that haven't been persisted
- Called by command handler after execution
- Events are then saved to event store

**`MarkEventsAsCommitted()`**:
- Clears uncommitted event list
- Called after events successfully persisted
- Prevents duplicate event saving

## Concrete Aggregates

### CommentAggregate

**Location**: [PlaneCrazy.Domain/Aggregates/CommentAggregate.cs](../src/PlaneCrazy.Domain/Aggregates/CommentAggregate.cs)

**Purpose**: Manages the lifecycle of comments on entities (aircraft, types, airports)

#### Properties

```csharp
public class CommentAggregate : AggregateRoot
{
    public override string Id { get; } = Guid.NewGuid().ToString();
    
    private string _entityType = string.Empty;
    private string _entityId = string.Empty;
    private string _text = string.Empty;
    private string? _user;
    private DateTime _createdAt;
    private DateTime? _lastEditedAt;
    private bool _isDeleted;
}
```

#### Commands Handled

1. **AddCommentCommand**
2. **EditCommentCommand**
3. **DeleteCommentCommand**

#### Events Emitted

1. **CommentAdded**
2. **CommentEdited**
3. **CommentDeleted**

#### Business Logic

**Add Comment**:
```csharp
public void AddComment(string entityType, string entityId, string text, string? user = null)
{
    // Validation
    if (string.IsNullOrWhiteSpace(entityType))
        throw new ArgumentException("Entity type is required", nameof(entityType));
    
    if (string.IsNullOrWhiteSpace(entityId))
        throw new ArgumentException("Entity ID is required", nameof(entityId));
    
    if (string.IsNullOrWhiteSpace(text))
        throw new ArgumentException("Comment text is required", nameof(text));
    
    // Emit event
    var @event = new CommentAdded
    {
        EntityType = entityType,
        EntityId = entityId,
        Text = text,
        User = user
    };
    
    AddEvent(@event);
}
```

**Edit Comment**:
```csharp
public void EditComment(string newText)
{
    // Business rule: Cannot edit deleted comment
    if (_isDeleted)
        throw new InvalidOperationException("Cannot edit a deleted comment");
    
    // Validation
    if (string.IsNullOrWhiteSpace(newText))
        throw new ArgumentException("Comment text is required", nameof(newText));
    
    // Emit event
    var @event = new CommentEdited
    {
        CommentId = Guid.Parse(Id),
        Text = newText
    };
    
    AddEvent(@event);
}
```

**Delete Comment**:
```csharp
public void DeleteComment()
{
    // Business rule: Cannot delete already deleted comment
    if (_isDeleted)
        throw new InvalidOperationException("Comment is already deleted");
    
    // Emit event
    var @event = new CommentDeleted
    {
        CommentId = Guid.Parse(Id)
    };
    
    AddEvent(@event);
}
```

#### Event Application

```csharp
protected override void Apply(DomainEvent @event)
{
    switch (@event)
    {
        case CommentAdded added:
            _entityType = added.EntityType;
            _entityId = added.EntityId;
            _text = added.Text;
            _user = added.User;
            _createdAt = added.OccurredAt;
            break;
            
        case CommentEdited edited:
            _text = edited.Text;
            _lastEditedAt = edited.OccurredAt;
            break;
            
        case CommentDeleted deleted:
            _isDeleted = true;
            break;
    }
}
```

#### Invariants

1. Comment text cannot be empty
2. Cannot edit deleted comments
3. Cannot delete already deleted comments
4. Entity type and ID are immutable after creation

---

### FavouriteAggregate

**Location**: [PlaneCrazy.Domain/Aggregates/FavouriteAggregate.cs](../src/PlaneCrazy.Domain/Aggregates/FavouriteAggregate.cs)

**Purpose**: Manages user favourites for aircraft, types, and airports

#### Properties

```csharp
public class FavouriteAggregate : AggregateRoot
{
    public override string Id { get; } = Guid.NewGuid().ToString();
    
    private string _favouriteType = string.Empty; // "Aircraft", "Type", or "Airport"
    private string _entityId = string.Empty;
    private bool _isFavourited = false;
    private DateTime? _favouritedAt;
    private DateTime? _unfavouritedAt;
}
```

#### Commands Handled

1. **FavouriteAircraftCommand**
2. **UnfavouriteAircraftCommand**
3. **FavouriteAircraftTypeCommand**
4. **UnfavouriteAircraftTypeCommand**
5. **FavouriteAirportCommand**
6. **UnfavouriteAirportCommand**

#### Events Emitted

1. **AircraftFavourited** / **AircraftUnfavourited**
2. **TypeFavourited** / **TypeUnfavourited**
3. **AirportFavourited** / **AirportUnfavourited**

#### Business Logic

**Favourite Aircraft**:
```csharp
public void FavouriteAircraft(string icao24, string? registration = null, string? typeCode = null)
{
    // Validation
    if (string.IsNullOrWhiteSpace(icao24))
        throw new ArgumentException("ICAO24 is required", nameof(icao24));
    
    // Business rule: Can favourite again even if already favourited
    // (Creates new favourite record)
    
    var @event = new AircraftFavourited
    {
        Icao24 = icao24.ToUpper(),
        Registration = registration?.Trim(),
        TypeCode = typeCode?.Trim()
    };
    
    AddEvent(@event);
}
```

**Unfavourite Aircraft**:
```csharp
public void UnfavouriteAircraft(string icao24)
{
    if (string.IsNullOrWhiteSpace(icao24))
        throw new ArgumentException("ICAO24 is required", nameof(icao24));
    
    // Note: No check for "already unfavourited"
    // Projection handles deduplication
    
    var @event = new AircraftUnfavourited
    {
        Icao24 = icao24.ToUpper()
    };
    
    AddEvent(@event);
}
```

**Similar methods** for types and airports

#### Event Application

```csharp
protected override void Apply(DomainEvent @event)
{
    switch (@event)
    {
        case AircraftFavourited aircraftFav:
            _favouriteType = "Aircraft";
            _entityId = aircraftFav.Icao24;
            _isFavourited = true;
            _favouritedAt = aircraftFav.OccurredAt;
            break;
            
        case AircraftUnfavourited aircraftUnfav:
            _isFavourited = false;
            _unfavouritedAt = aircraftUnfav.OccurredAt;
            break;
            
        case TypeFavourited typeFav:
            _favouriteType = "Type";
            _entityId = typeFav.TypeCode;
            _isFavourited = true;
            _favouritedAt = typeFav.OccurredAt;
            break;
            
        // ... similar for other events
    }
}
```

#### Invariants

1. Entity ID cannot be empty
2. Favourite type must be "Aircraft", "Type", or "Airport"
3. Favouriting/unfavouriting is idempotent at projection level

---

## Aggregate Lifecycle

### 1. Creation

```csharp
// Create new aggregate
var aggregate = new CommentAggregate();

// Execute command on aggregate
aggregate.AddComment("Aircraft", "ABC123", "Great aircraft!", "john.doe");

// Aggregate now has uncommitted events
var events = aggregate.GetUncommittedEvents();
// events: [CommentAdded]
```

### 2. Persistence

```csharp
// Command handler saves events to event store
foreach (var @event in aggregate.GetUncommittedEvents())
{
    await eventStore.AppendAsync(@event);
}

// Mark as committed
aggregate.MarkEventsAsCommitted();
```

### 3. Loading from History

```csharp
// Load all events for aggregate
var events = await eventStore.GetByAggregateIdAsync(aggregateId);

// Reconstruct aggregate from events
var aggregate = new CommentAggregate();
aggregate.LoadFromHistory(events);

// Aggregate state is now hydrated
// No uncommitted events
```

### 4. Modification

```csharp
// Load existing aggregate
var events = await eventStore.GetByAggregateIdAsync(commentId);
var aggregate = new CommentAggregate();
aggregate.LoadFromHistory(events);

// Execute command
aggregate.EditComment("Updated comment text");

// Save new events
foreach (var @event in aggregate.GetUncommittedEvents())
{
    await eventStore.AppendAsync(@event);
}

aggregate.MarkEventsAsCommitted();
```

## Aggregate Design Patterns

### Command-Event Flow

```
User Action
    ↓
Command (Intent to change)
    ↓
Command Handler
    ↓
Load Aggregate from Event Store
    ↓
Execute Business Logic on Aggregate
    ↓
Aggregate Emits Event (Fact)
    ↓
Save Event to Event Store
    ↓
Event Dispatcher
    ↓
Projections Updated
```

### Event Sourcing Benefits

1. **Complete History**: Every state change is recorded
2. **Audit Trail**: Know exactly what happened and when
3. **Time Travel**: Reconstruct state at any point in time
4. **Event Replay**: Rebuild projections from scratch
5. **Debugging**: Reproduce issues by replaying events
6. **Bi-temporal Data**: Track both business time and system time

### Consistency Boundaries

Each aggregate is a **consistency boundary**:

**Within Aggregate**:
- Strong consistency
- All invariants enforced
- Atomic changes

**Between Aggregates**:
- Eventual consistency
- Separate transactions
- Domain events coordinate changes

**Example**:
```csharp
// ✅ Good: Single aggregate, atomic
var comment = new CommentAggregate();
comment.AddComment("Aircraft", "ABC123", "Great!");

// ❌ Bad: Don't modify multiple aggregates in one transaction
var comment1 = new CommentAggregate();
var comment2 = new CommentAggregate();
comment1.AddComment(...);
comment2.AddComment(...); // Should be separate transactions
```

## Advanced Patterns

### Snapshot Pattern

For aggregates with many events, use snapshots:

```csharp
public class AggregateSnapshot
{
    public string AggregateId { get; set; }
    public int Version { get; set; }
    public string State { get; set; } // JSON serialized state
    public DateTime SnapshotAt { get; set; }
}

// Load aggregate with snapshot
var snapshot = await snapshotStore.GetLatestSnapshotAsync(aggregateId);
var aggregate = new CommentAggregate();

if (snapshot != null)
{
    aggregate.LoadFromSnapshot(snapshot);
    var eventsAfterSnapshot = await eventStore.GetEventsAfterVersionAsync(
        aggregateId, snapshot.Version);
    aggregate.LoadFromHistory(eventsAfterSnapshot);
}
else
{
    var allEvents = await eventStore.GetByAggregateIdAsync(aggregateId);
    aggregate.LoadFromHistory(allEvents);
}
```

### Saga Pattern

Coordinate multi-aggregate transactions:

```csharp
public class FavouriteAircraftSaga
{
    public async Task Handle(FavouriteAircraftCommand command)
    {
        // Step 1: Favourite aircraft
        var favouriteAggregate = new FavouriteAggregate();
        favouriteAggregate.FavouriteAircraft(command.Icao24);
        await SaveAggregate(favouriteAggregate);
        
        // Step 2: Add automatic comment
        var commentAggregate = new CommentAggregate();
        commentAggregate.AddComment("Aircraft", command.Icao24, 
            "Automatically added to favourites");
        await SaveAggregate(commentAggregate);
        
        // If Step 2 fails, compensating action for Step 1
        // (in real implementation, use saga state machine)
    }
}
```

### Process Manager

Manage long-running business processes:

```csharp
public class AircraftTrackingProcess
{
    private string _icao24;
    private DateTime _firstSeen;
    private DateTime? _lastSeen;
    private ProcessState _state;
    
    public void Handle(AircraftFirstSeen @event)
    {
        _icao24 = @event.Icao24;
        _firstSeen = @event.OccurredAt;
        _state = ProcessState.Tracking;
    }
    
    public void Handle(AircraftLastSeen @event)
    {
        _lastSeen = @event.OccurredAt;
        _state = ProcessState.Completed;
        
        // Trigger completion actions
        SendNotificationIfFavourited();
    }
}
```

## Testing Aggregates

### Unit Testing Pattern

```csharp
[Test]
public void AddComment_ValidData_EmitsCommentAddedEvent()
{
    // Arrange
    var aggregate = new CommentAggregate();
    
    // Act
    aggregate.AddComment("Aircraft", "ABC123", "Test comment");
    
    // Assert
    var events = aggregate.GetUncommittedEvents().ToList();
    Assert.That(events, Has.Count.EqualTo(1));
    Assert.That(events[0], Is.InstanceOf<CommentAdded>());
    
    var commentAdded = (CommentAdded)events[0];
    Assert.That(commentAdded.EntityType, Is.EqualTo("Aircraft"));
    Assert.That(commentAdded.EntityId, Is.EqualTo("ABC123"));
    Assert.That(commentAdded.Text, Is.EqualTo("Test comment"));
}

[Test]
public void EditComment_OnDeletedComment_ThrowsException()
{
    // Arrange
    var aggregate = new CommentAggregate();
    aggregate.AddComment("Aircraft", "ABC123", "Test");
    aggregate.DeleteComment();
    aggregate.MarkEventsAsCommitted();
    
    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => 
        aggregate.EditComment("Updated text"));
}
```

### Event Replay Testing

```csharp
[Test]
public void LoadFromHistory_ReconstructsState()
{
    // Arrange
    var events = new List<DomainEvent>
    {
        new CommentAdded 
        { 
            EntityType = "Aircraft", 
            EntityId = "ABC123", 
            Text = "Original" 
        },
        new CommentEdited 
        { 
            CommentId = Guid.NewGuid(), 
            Text = "Edited" 
        }
    };
    
    // Act
    var aggregate = new CommentAggregate();
    aggregate.LoadFromHistory(events);
    
    // Assert
    Assert.That(aggregate.Version, Is.EqualTo(2));
    // Verify internal state through behavior
}
```

## Best Practices

### 1. Keep Aggregates Small

**Good**:
```csharp
// Small, focused aggregate
public class CommentAggregate : AggregateRoot
{
    // Only comment-related logic
}
```

**Bad**:
```csharp
// Too large, multiple responsibilities
public class AircraftAggregate : AggregateRoot
{
    // Aircraft data
    // Comments
    // Favourites
    // Tracking history
    // User preferences
}
```

### 2. Protect Invariants

```csharp
public void EditComment(string newText)
{
    // Enforce invariants
    if (_isDeleted)
        throw new InvalidOperationException("Cannot edit deleted comment");
    
    if (string.IsNullOrWhiteSpace(newText))
        throw new ArgumentException("Text required");
    
    // Business logic
    var @event = new CommentEdited { Text = newText };
    AddEvent(@event);
}
```

### 3. Use Meaningful Events

**Good**:
```csharp
// Specific, intent-revealing events
public class CommentAddedToAircraft : DomainEvent { }
public class CommentMarkedAsInappropriate : DomainEvent { }
```

**Bad**:
```csharp
// Generic, unclear events
public class CommentUpdated : DomainEvent { }
public class DataChanged : DomainEvent { }
```

### 4. Immutable Events

```csharp
// ✅ Immutable properties
public class CommentAdded : DomainEvent
{
    public required string Text { get; init; }
}

// ❌ Mutable properties
public class CommentAdded : DomainEvent
{
    public string Text { get; set; }
}
```

### 5. Event Versioning

```csharp
// V1
public class CommentAdded : DomainEvent
{
    public string Text { get; set; }
}

// V2 - Additive change (backward compatible)
public class CommentAdded : DomainEvent
{
    public string Text { get; set; }
    public string? Sentiment { get; set; } // New optional field
}

// V3 - Breaking change (new event type)
public class CommentAddedV2 : DomainEvent
{
    // Completely new structure
}
```

### 6. Separate Read and Write Models

- **Aggregates**: Write model (commands, business logic)
- **Projections**: Read model (queries, optimized for display)
- **Don't query aggregates**: Load from projections instead

## Common Pitfalls

### 1. Aggregate Anemia

**Antipattern**: Aggregates with only getters/setters
```csharp
// ❌ Anemic aggregate
public class CommentAggregate
{
    public string Text { get; set; }
    public bool IsDeleted { get; set; }
}

// Logic in service layer (wrong!)
public class CommentService
{
    public void DeleteComment(CommentAggregate comment)
    {
        comment.IsDeleted = true; // Business logic outside aggregate
    }
}
```

**Solution**: Put logic in aggregate
```csharp
// ✅ Rich aggregate
public class CommentAggregate : AggregateRoot
{
    private bool _isDeleted;
    
    public void DeleteComment()
    {
        if (_isDeleted)
            throw new InvalidOperationException("Already deleted");
        
        AddEvent(new CommentDeleted { ... });
    }
}
```

### 2. Large Aggregates

**Problem**: Too many entities in one aggregate

**Solution**: Split into smaller aggregates with eventual consistency

### 3. Ignoring Invariants

**Problem**: Not enforcing business rules

**Solution**: Validate in aggregate methods before emitting events

### 4. Event Coupling

**Problem**: Events containing references to other aggregates

**Solution**: Use IDs only, maintain loose coupling

## Future Enhancements

1. **Snapshot Support**: Optimize loading of large aggregates
2. **Saga Coordinator**: Multi-aggregate transactions
3. **Process Managers**: Long-running workflows
4. **Event Upcasting**: Handle event schema evolution
5. **Aggregate Repository**: Generic loading/saving
6. **Concurrency Control**: Optimistic locking with version numbers

---

*Last Updated: January 24, 2026*
