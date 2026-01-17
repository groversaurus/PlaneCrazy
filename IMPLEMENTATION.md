# CommentsProjection Implementation

## Overview

This implementation provides a **CommentsProjection** that rebuilds the current comment list for an entity by replaying comment events, following event sourcing architectural patterns.

## Architecture

### Event Sourcing Infrastructure

The implementation includes a basic event sourcing infrastructure:

- **IEvent**: Interface defining the core properties of any event (EventId, Timestamp, EntityId)
- **BaseEvent**: Abstract base class implementing IEvent with immutable properties using `init` accessors

### Comment Domain

#### Comment Model
The `Comment` class represents the current state of a comment with:
- **Immutable properties**: CommentId, EntityId, Author, CreatedAt (set once at creation)
- **Mutable properties**: Text, UpdatedAt, IsDeleted (updated as events are replayed)

#### Comment Events
Three event types capture all comment lifecycle operations:

1. **CommentAddedEvent**: Fired when a new comment is created
   - Contains: CommentId, Text, Author

2. **CommentUpdatedEvent**: Fired when comment text is modified
   - Contains: CommentId, Text

3. **CommentDeletedEvent**: Fired when a comment is soft-deleted
   - Contains: CommentId

All events inherit from BaseEvent and have immutable properties to ensure event integrity.

### CommentsProjection

The main projection class that rebuilds comment state from a stream of events.

#### Key Features

1. **Event Replay**: The `RebuildFromEvents()` method processes events in chronological order (sorted by Timestamp) to rebuild the current state
2. **State Management**: Maintains an internal dictionary of comments indexed by CommentId
3. **Soft Deletes**: Deleted comments are marked with `IsDeleted = true` rather than removed
4. **Entity Filtering**: Supports multiple entities, with queries filtered by EntityId
5. **Resilience**: Silently ignores update/delete events for non-existent comments (handles partial event streams)

#### Public API

- `RebuildFromEvents(IEnumerable<IEvent> events)`: Clears existing state and rebuilds from event stream
- `GetComments(string entityId)`: Returns active (non-deleted) comments for an entity, ordered by creation time
- `GetComment(Guid commentId)`: Returns a specific comment by ID (returns null if deleted or not found)

## Usage Example

```csharp
// Create projection
var projection = new CommentsProjection();

// Define events
var events = new List<IEvent>
{
    new CommentAddedEvent
    {
        CommentId = Guid.NewGuid(),
        EntityId = "flight-123",
        Text = "Great flight tracking!",
        Author = "John Doe",
        Timestamp = DateTime.UtcNow
    },
    new CommentAddedEvent
    {
        CommentId = Guid.NewGuid(),
        EntityId = "flight-123",
        Text = "Very useful app",
        Author = "Jane Smith",
        Timestamp = DateTime.UtcNow.AddMinutes(5)
    }
};

// Rebuild state from events
projection.RebuildFromEvents(events);

// Query comments
var comments = projection.GetComments("flight-123");
// Returns 2 comments ordered by creation time
```

## Design Decisions

### Why Mutable Comment State?
The `Comment` class has mutable properties for `Text`, `UpdatedAt`, and `IsDeleted` because these represent the *current projection state* that gets updated as events are replayed. This is different from the events themselves, which are immutable.

### Why Silent Failure on Update/Delete?
The projection silently ignores update and delete events for non-existent comments. This design makes the projection:
- **Resilient**: Can handle partial event streams without failing
- **Idempotent**: Can be replayed multiple times with the same result
- **Flexible**: Works even if some add events are missing from the stream

### Why Soft Deletes?
Comments are marked as deleted rather than removed to:
- Maintain complete history
- Allow for potential "undelete" functionality
- Keep referential integrity

## Testing

The implementation includes comprehensive unit tests covering:
- Empty event streams
- Single and multiple comment additions
- Comment updates
- Comment deletions (soft delete behavior)
- Out-of-order event processing (validates timestamp ordering)
- Multiple entity support
- Multiple rebuild operations (validates state clearing)
- Comment ordering by creation time

All 9 tests pass successfully.

## Project Structure

```
PlaneCrazy.sln
├── PlaneCrazy.Core/
│   ├── Events/
│   │   ├── IEvent.cs
│   │   └── BaseEvent.cs
│   ├── Comments/
│   │   ├── Comment.cs
│   │   ├── CommentAddedEvent.cs
│   │   ├── CommentUpdatedEvent.cs
│   │   └── CommentDeletedEvent.cs
│   └── Projections/
│       └── CommentsProjection.cs
└── PlaneCrazy.Tests/
    └── CommentsProjectionTests.cs
```

## Build and Test

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Build succeeded with 0 warnings, 0 errors
# All 9 tests passed
```
