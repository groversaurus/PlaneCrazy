# Command Handler Architecture

## Overview

PlaneCrazy implements a **Command Query Responsibility Segregation (CQRS)** pattern with **Event Sourcing**. Command handlers are the core components responsible for processing write operations (commands) in the system.

## Architecture Components

### 1. Commands

Commands represent user intent to perform an action. They are immutable data structures that encapsulate all information needed to execute a specific operation.

**Location**: `PlaneCrazy.Domain/Commands/`

**Key Characteristics**:
- Inherit from the `Command` base class
- Include metadata (CommandId, CreatedAt, IssuedBy, CorrelationId)
- Implement a `Validate()` method for command validation
- Are immutable (use `init` properties)

**Example**:
```csharp
public class AddCommentCommand : Command
{
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public required string Text { get; init; }
    public string? User { get; init; }
    
    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Text))
            throw new ArgumentException("Text cannot be empty.");
    }
}
```

### 2. Aggregates

Aggregates are domain entities that maintain consistency boundaries and enforce business rules. They rebuild their state from a stream of domain events.

**Location**: `PlaneCrazy.Domain/Aggregates/`

**Key Aggregates**:
- **AggregateRoot**: Base class providing event sourcing infrastructure
- **CommentAggregate**: Manages comment lifecycle (add, edit, delete)
- **FavouriteAggregate**: Manages favourite entities (aircraft, types, airports)

**Key Responsibilities**:
- **State Reconstruction**: Rebuild current state by replaying historical events
- **Business Rule Enforcement**: Validate that commands can be executed given current state
- **Event Generation**: Produce new domain events when commands are successfully executed
- **Consistency**: Maintain transactional boundaries and data integrity

**Example**:
```csharp
public class CommentAggregate : AggregateRoot
{
    private bool _isDeleted;
    
    public void EditComment(EditCommentCommand command)
    {
        // Business rule validation
        if (_isDeleted)
            throw new InvalidOperationException("Cannot edit a deleted comment.");
        
        // Generate event
        var @event = new CommentEdited { ... };
        ApplyChange(@event);
    }
    
    protected override void Apply(DomainEvent @event)
    {
        // Update internal state based on event
        switch (@event)
        {
            case CommentEdited edited:
                _text = edited.Text;
                break;
        }
    }
}
```

### 3. Command Handlers

Command handlers orchestrate the execution of commands. They follow a consistent pattern:

**Location**: `PlaneCrazy.Domain/CommandHandlers/`

**Execution Flow**:

1. **Validate Command**: Call `command.Validate()` to ensure the command is well-formed
2. **Load Event Stream**: Fetch all historical events for the aggregate from the event store
3. **Rebuild State**: Create an aggregate and replay events to reconstruct current state
4. **Execute Command**: Call the appropriate method on the aggregate, which:
   - Validates business rules
   - Generates new domain events
5. **Persist Events**: Append new events to the event store (immutable append-only log)
6. **Update Projections**: Rebuild read models to reflect the new state

**Example**:
```csharp
public class EditCommentCommandHandler : ICommandHandler<EditCommentCommand>
{
    private readonly IEventStore _eventStore;
    private readonly CommentProjection _commentProjection;
    
    public async Task HandleAsync(EditCommentCommand command)
    {
        // 1. Validate
        command.Validate();
        
        // 2. Load event stream
        var events = await _eventStore.ReadAllEventsAsync();
        var commentEvents = events.Where(e => IsCommentEvent(e, command.CommentId));
        
        // 3. Rebuild state
        var aggregate = new CommentAggregate(command.CommentId);
        aggregate.LoadFromHistory(commentEvents);
        
        // 4. Execute command
        aggregate.EditComment(command);
        
        // 5. Persist events
        foreach (var @event in aggregate.GetUncommittedEvents())
        {
            await _eventStore.AppendEventAsync(@event);
        }
        
        // 6. Update projections
        await _commentProjection.RebuildForEntityAsync(
            command.EntityType, command.EntityId);
    }
}
```

### 4. Events

Events represent facts that have occurred in the system. They are immutable records of state changes.

**Location**: `PlaneCrazy.Domain/Events/`

**Key Characteristics**:
- Inherit from `DomainEvent` base class
- Are immutable (past tense naming: "CommentAdded", "AircraftFavourited")
- Include metadata (Id, OccurredAt, EventType)
- Stored persistently in the event store

### 5. Event Store

The event store is an append-only log that persists all domain events.

**Location**: `PlaneCrazy.Infrastructure/EventStore/`

**Implementation**: `JsonFileEventStore` - stores events as JSON files

**Key Operations**:
- `AppendEventAsync()`: Add a new event to the store
- `ReadAllEventsAsync()`: Retrieve all events in chronological order
- `ReadEventsAsync()`: Retrieve events with filtering

### 6. Projections

Projections are read models built from events. They provide optimized views for queries.

**Location**: `PlaneCrazy.Infrastructure/Projections/`

**Key Projections**:
- **CommentProjection**: Builds the current state of all comments
- **FavouriteProjection**: Builds the current state of all favourites

**Responsibilities**:
- Replay events to rebuild read models
- Handle specific event types to update state
- Provide query-optimized data structures

## Command Handler Implementations

### Comment Handlers

| Handler | Command | Events Emitted | Business Rules |
|---------|---------|----------------|----------------|
| AddCommentCommandHandler | AddCommentCommand | CommentAdded | - Text must not be empty<br>- Text max 5000 chars |
| EditCommentCommandHandler | EditCommentCommand | CommentEdited | - Comment must exist<br>- Comment must not be deleted<br>- Text must not be empty |
| DeleteCommentCommandHandler | DeleteCommentCommand | CommentDeleted | - Comment must exist<br>- Comment must not already be deleted |

### Favourite Handlers

| Handler | Command | Events Emitted | Business Rules |
|---------|---------|----------------|----------------|
| FavouriteAircraftCommandHandler | FavouriteAircraftCommand | AircraftFavourited | - Valid ICAO24 format (6 hex chars)<br>- Aircraft not already favourited |
| UnfavouriteAircraftCommandHandler | UnfavouriteAircraftCommand | AircraftUnfavourited | - Aircraft must be favourited |
| FavouriteAircraftTypeCommandHandler | FavouriteAircraftTypeCommand | TypeFavourited | - Valid type code (2-10 chars)<br>- Type not already favourited |
| UnfavouriteAircraftTypeCommandHandler | UnfavouriteAircraftTypeCommand | TypeUnfavourited | - Type must be favourited |
| FavouriteAirportCommandHandler | FavouriteAirportCommand | AirportFavourited | - Valid ICAO code (4 uppercase letters)<br>- Airport not already favourited |
| UnfavouriteAirportCommandHandler | UnfavouriteAirportCommand | AirportUnfavourited | - Airport must be favourited |

## Dependency Injection

All command handlers are registered in the DI container as **Transient** services since they are stateless and created per request.

**Registration** (in `ServiceCollectionExtensions.cs`):
```csharp
services.AddTransient<ICommandHandler<AddCommentCommand>, AddCommentCommandHandler>();
services.AddTransient<ICommandHandler<EditCommentCommand>, EditCommentCommandHandler>();
// ... etc
```

## Usage Example

```csharp
// Resolve handler from DI container
var handler = serviceProvider.GetRequiredService<ICommandHandler<AddCommentCommand>>();

// Create command
var command = new AddCommentCommand
{
    EntityType = "Aircraft",
    EntityId = "A1B2C3",
    Text = "Great aircraft!",
    User = "john.doe"
};

// Execute command
await handler.HandleAsync(command);
```

## Benefits of This Architecture

1. **Event Sourcing**: Complete audit trail of all changes
2. **State Reconstruction**: Can rebuild state at any point in time
3. **Separation of Concerns**: Clear boundaries between read and write models
4. **Consistency**: Aggregates enforce business rules and transactional boundaries
5. **Scalability**: Read and write models can be scaled independently
6. **Testability**: Each component has clear, testable responsibilities
7. **Flexibility**: Easy to add new projections without modifying write side

## Best Practices

1. **Validate Early**: Commands should validate inputs before reaching aggregates
2. **Single Responsibility**: Each command handler processes one command type
3. **Immutability**: Commands and events are immutable
4. **Idempotency**: Handlers should be designed to handle duplicate commands safely
5. **Event Versioning**: Include version information in events for future schema evolution
6. **Error Handling**: Use exceptions for business rule violations
7. **Projection Updates**: Always update projections after persisting events

## Future Enhancements

Potential improvements to the command handler architecture:

1. **Stream/Aggregate ID Support**: Add StreamId property to DomainEvent base class to enable efficient event retrieval by aggregate
2. **Optimized Event Loading**: Implement storage-level filtering in EventStore to avoid loading all events into memory
3. **Command Handler Base Class**: Extract common event persistence logic into a base class to reduce code duplication
4. **Command Bus**: Central dispatcher for routing commands to handlers
5. **Command Middleware**: Cross-cutting concerns (logging, validation, authorization)
6. **Optimistic Concurrency**: Version checking to prevent conflicts
7. **Sagas**: Coordinate long-running business processes across aggregates
8. **Snapshot Support**: Optimize state reconstruction for large event streams
9. **Event Versioning**: Handle event schema evolution over time
10. **Integration Events**: Publish events to external systems

## Performance Considerations

The current implementation loads all events from the event store and filters in memory. This is acceptable for:
- Small to medium event stores (< 10,000 events)
- Development and testing environments
- Initial production deployments

For larger deployments, consider implementing storage-level filtering by:
- Adding StreamId/AggregateId to the DomainEvent base class
- Implementing indexed event retrieval in the EventStore
- Using a database-backed event store instead of file-based storage
