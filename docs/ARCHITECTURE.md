# PlaneCrazy Architecture

## Overview

PlaneCrazy is a C# console application built using **Event Sourcing** and **CQRS** (Command Query Responsibility Segregation) patterns. The application tracks ADS-B aircraft data from adsb.fi and provides features for favoriting and commenting on aircraft, types, and airports.

## Architecture Principles

- **Clean Architecture**: Clear separation between domain logic, infrastructure, and presentation layers
- **Event Sourcing**: All state changes are captured as immutable events in an append-only log
- **CQRS**: Separate models for write operations (commands) and read operations (queries)
- **Dependency Injection**: Loose coupling through interface-based design
- **Domain-Driven Design**: Rich domain model with aggregates, entities, and value objects

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                      Console Application                         │
│  (PlaneCrazy.Console - Presentation Layer)                      │
│  - User Interface                                                │
│  - Service Resolution                                            │
│  - Menu System                                                   │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ├──────────────────┐
                     │                  │
         ┌───────────▼──────┐    ┌──────▼─────────────┐
         │  Query Services  │    │  Command Handlers  │
         │  (Read Side)     │    │  (Write Side)      │
         └───────────┬──────┘    └──────┬─────────────┘
                     │                  │
         ┌───────────▼──────────────────▼───────────────┐
         │         Domain Layer                          │
         │  (PlaneCrazy.Domain - Core Business Logic)   │
         │  - Entities          - Events                │
         │  - Aggregates        - Commands              │
         │  - Interfaces        - Validation            │
         └───────────┬──────────────────┬───────────────┘
                     │                  │
         ┌───────────▼──────────────────▼───────────────┐
         │      Infrastructure Layer                     │
         │  (PlaneCrazy.Infrastructure)                 │
         │  - Event Store       - Event Dispatcher      │
         │  - Repositories      - Projections           │
         │  - HTTP Services     - Background Services   │
         └───────────┬──────────────────┬───────────────┘
                     │                  │
         ┌───────────▼──────┐    ┌──────▼─────────────┐
         │   File System    │    │   External API     │
         │   (JSON Files)   │    │   (adsb.fi)        │
         └──────────────────┘    └────────────────────┘
```

## Layer Responsibilities

### Console Layer (PlaneCrazy.Console)
**Purpose**: User interface and application entry point

**Components**:
- `Program.cs`: Main application, DI configuration, menu system
- Interactive console UI with menu navigation
- Service resolution from DI container
- Background service hosting

**Dependencies**:
- References Domain and Infrastructure layers
- Uses Microsoft.Extensions.Hosting for background services
- Uses Microsoft.Extensions.DependencyInjection for service resolution

### Domain Layer (PlaneCrazy.Domain)
**Purpose**: Core business logic and domain models

**Components**:
- **Entities**: `Aircraft`, `AircraftType`, `Airport`, `Comment`, `Favourite`
- **Events**: `AircraftFavourited`, `CommentAdded`, `AircraftPositionUpdated`, etc.
- **Commands**: `AddCommentCommand`, `FavouriteAircraftCommand`, etc.
- **Aggregates**: `AggregateRoot`, `CommentAggregate`, `FavouriteAggregate`
- **Interfaces**: Contracts for repositories, services, and infrastructure
- **Validation**: Command and event validation framework
- **Query Results**: DTOs for read operations

**Characteristics**:
- No dependencies on other layers
- Pure C# with no external packages
- Contains business rules and invariants
- Defines contracts but doesn't implement infrastructure

### Infrastructure Layer (PlaneCrazy.Infrastructure)
**Purpose**: Implementation of domain interfaces and external integrations

**Components**:
- **Event Store**: JSON file-based event persistence
- **Repositories**: JSON file-based data storage with thread-safe operations
- **Projections**: Read model builders (FavouriteProjection, CommentProjection, etc.)
- **Command Handlers**: Command processing and event generation
- **Query Services**: Read-side query implementations
- **HTTP Services**: External API integration (adsb.fi)
- **Background Services**: Automated polling and event generation
- **Event Dispatcher**: Coordinates event writing and projection updates

**Dependencies**:
- References Domain layer
- Uses Microsoft.Extensions packages for DI, logging, hosting
- Uses System.Text.Json for serialization

## Data Flow Diagrams

### Command Flow (Write Operations)

```
User Input
    ↓
Console Application
    ↓
Command Creation (e.g., AddCommentCommand)
    ↓
Command Validation
    ↓
Command Handler
    ↓
┌──────────────────────────────┐
│  1. Load Event Stream        │
│  2. Rebuild Aggregate State  │
│  3. Execute Business Logic   │
│  4. Generate Domain Events   │
└──────────────┬───────────────┘
               ↓
Event Dispatcher
    ↓
┌──────────────────────────────┐
│  1. Validate Event           │
│  2. Append to Event Store    │
│  3. Update All Projections   │
└──────────────┬───────────────┘
               ↓
Projections Updated (Read Models)
```

### Query Flow (Read Operations)

```
User Request
    ↓
Console Application
    ↓
Query Service (e.g., ICommentQueryService)
    ↓
Repository (Read from Projection)
    ↓
Query Result (DTO)
    ↓
Display to User
```

### Event Flow

```
Domain Event Created
    ↓
Event Dispatcher.DispatchAsync()
    ├──→ Event Store (Append to JSON file)
    │
    └──→ All Registered Projections
         ├──→ FavouriteProjection.ApplyEventAsync()
         ├──→ CommentProjection.ApplyEventAsync()
         └──→ AircraftStateProjection.ApplyEventAsync()
    ↓
Read Models Updated
```

### Background Polling Flow

```
Timer Trigger (every 30 seconds)
    ↓
BackgroundAdsBPoller.PollAndProcessAircraftAsync()
    ↓
Fetch Aircraft from adsb.fi API
    ↓
Compare with Existing Aircraft in Repository
    ↓
┌───────────────────────────────────────┐
│  Detect Changes:                      │
│  - New Aircraft  → AircraftFirstSeen  │
│  - Position Changed → PositionUpdated │
│  - Identity Changed → IdentityUpdated │
│  - Missing Aircraft → AircraftLastSeen│
└───────────────┬───────────────────────┘
                ↓
Emit Domain Events via Event Dispatcher
    ↓
Events Stored & Projections Updated
```

## Event Sourcing Implementation

### Core Concepts

1. **Events are the Source of Truth**: All state changes are stored as events
2. **Append-Only Store**: Events are never modified or deleted
3. **State Reconstruction**: Current state is rebuilt by replaying events
4. **Audit Trail**: Complete history of all changes
5. **Temporal Queries**: Can rebuild state at any point in time

### Event Store Structure

```
Documents/PlaneCrazy/Events/
├── 20260124143022456_3fa85f64-5717-4562-b3fc-2c963f66afa6.json
├── 20260124143023789_7c9e6679-7425-40de-944b-e07fc1f90ae7.json
└── ... (chronologically ordered)
```

Each file contains:
```json
{
  "eventType": "CommentAdded",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "occurredAt": "2026-01-24T14:30:22.456Z",
    "eventType": "CommentAdded",
    "entityType": "Aircraft",
    "entityId": "A1B2C3",
    "text": "Great aircraft!",
    "user": "john.doe"
  }
}
```

### Projection Mechanism

Projections are denormalized read models built from events:

```csharp
public interface IProjection
{
    Task<bool> ApplyEventAsync(DomainEvent domainEvent);
    string ProjectionName { get; }
}
```

**Projection Update Process**:
1. Event is appended to event store
2. Event Dispatcher calls `ApplyEventAsync()` on each projection
3. Projection updates its read model based on event type
4. Read model is persisted to repository

**Projection Rebuild**:
- Can be triggered manually or on startup
- Clears existing read model
- Replays all events from event store in chronological order
- Ensures eventual consistency

## CQRS Implementation

### Write Side (Commands)

**Components**:
- Command objects (immutable DTOs)
- Command handlers
- Aggregates (enforce business rules)
- Domain events

**Flow**:
```
Command → Validation → Handler → Aggregate → Events → Event Store
```

**Example**:
```csharp
var command = new AddCommentCommand
{
    EntityType = "Aircraft",
    EntityId = "A1B2C3",
    Text = "Great aircraft!"
};

await commandHandler.HandleAsync(command);
```

### Read Side (Queries)

**Components**:
- Query services (interfaces)
- Query service implementations
- Repositories (read from projections)
- Query result DTOs

**Flow**:
```
Query → Query Service → Repository (Projection) → Query Result DTO
```

**Example**:
```csharp
var comments = await commentQueryService
    .GetActiveCommentsForEntityAsync("Aircraft", "A1B2C3");
```

### Separation Benefits

1. **Independent Scaling**: Read and write can scale separately
2. **Optimized Models**: Different models for different use cases
3. **Performance**: Read models denormalized for fast queries
4. **Flexibility**: Easy to add new projections without affecting writes

## Aggregate Pattern

### AggregateRoot Base Class

```csharp
public abstract class AggregateRoot
{
    // Tracks version and uncommitted events
    // Loads from historical event stream
    // Applies new events to state
    protected abstract void Apply(DomainEvent @event);
}
```

### Aggregate Responsibilities

1. **Consistency Boundary**: Enforces business rules
2. **State Management**: Rebuilds state from events
3. **Event Generation**: Creates new events when state changes
4. **Validation**: Ensures commands are valid before generating events

### Example: CommentAggregate

```csharp
public class CommentAggregate : AggregateRoot
{
    private bool _isDeleted;
    private string _text;
    
    public void EditComment(EditCommentCommand command)
    {
        if (_isDeleted)
            throw new InvalidOperationException("Cannot edit deleted comment");
        
        var @event = new CommentEdited { ... };
        ApplyChange(@event); // Applies to state and tracks as uncommitted
    }
    
    protected override void Apply(DomainEvent @event)
    {
        switch (@event)
        {
            case CommentAdded added:
                _text = added.Text;
                break;
            case CommentEdited edited:
                _text = edited.Text;
                break;
            case CommentDeleted:
                _isDeleted = true;
                break;
        }
    }
}
```

## Repository Pattern

### File-Based Implementation

All repositories inherit from `JsonFileRepository<T>`:

```csharp
public abstract class JsonFileRepository<T> : IRepository<T>
{
    // Thread-safe operations with SemaphoreSlim
    // JSON serialization with System.Text.Json
    // Stores in Documents/PlaneCrazy/Data/
}
```

### Thread Safety

- Uses `SemaphoreSlim` for synchronization
- Ensures atomic read-modify-write operations
- Safe for concurrent access from background services and UI

### Storage Location

```
Documents/PlaneCrazy/Data/
├── aircraft.json
├── favourites.json
├── comments.json
└── airports.json
```

## Dependency Injection Architecture

### Service Registration

All services registered in `ServiceCollectionExtensions.AddPlaneCrazyInfrastructure()`:

```csharp
services.AddSingleton<IEventStore, JsonFileEventStore>();
services.AddSingleton<IEventDispatcher, EventDispatcher>();
services.AddSingleton<AircraftRepository>();
services.AddSingleton<FavouriteProjection>();
services.AddSingleton<IAircraftDataService, AdsbFiAircraftService>();
services.AddTransient<ICommandHandler<AddCommentCommand>, AddCommentCommandHandler>();
services.AddHostedService<BackgroundAdsBPoller>();
```

### Service Lifetimes

- **Singleton**: Event store, repositories, projections, services (stateful or expensive to create)
- **Transient**: Command handlers (stateless, created per request)
- **Scoped**: Not used (console app doesn't have request scopes)

### Dependency Graph

```
Program (Entry Point)
    ↓
IHost (Background Service Host)
    ↓
ServiceProvider (DI Container)
    ├→ IEventStore
    ├→ IEventDispatcher
    │    ├→ IEventStore
    │    └→ IEnumerable<IProjection>
    ├→ Repositories
    ├→ Projections
    ├→ Query Services
    ├→ Command Handlers
    └→ Background Services
```

## Background Services

### Architecture

Uses `Microsoft.Extensions.Hosting.IHostedService` for background tasks:

```csharp
public class BackgroundAdsBPoller : IHostedService
{
    // Runs on Timer (every 30 seconds)
    // Non-blocking (doesn't interfere with UI)
    // Graceful shutdown on app exit
}
```

### Execution Flow

1. Application starts, calls `StartAsync()`
2. Timer triggers `DoWork()` periodically
3. Fetches aircraft data from adsb.fi
4. Compares with repository state
5. Emits appropriate domain events
6. Events stored and projections updated
7. Application exits, calls `StopAsync()`

## Error Handling Strategy

### Validation Errors
- Caught at command/event validation
- Throws `ValidationException` with detailed messages
- Prevents invalid data from entering event store

### API Failures
- Logged but don't stop background polling
- Returns empty results on failure
- Continues polling on next cycle

### File I/O Errors
- Logged with full exception details
- Corrupted event files skipped during read
- Application continues with available data

### Projection Errors
- Logged individually per projection
- Failed projections don't block others
- Event still persisted to event store
- Projections can be rebuilt later

## Performance Considerations

### Event Store
- Sequential file writes (single semaphore)
- File names include timestamp for ordering
- Read operations load all files (acceptable for small-medium stores)

### Projections
- Updated synchronously after event persistence
- Multiple projections processed sequentially
- Can be optimized with parallel processing if needed

### Repositories
- In-memory caching via singleton lifetime
- Thread-safe with semaphore locks
- JSON serialization overhead acceptable for current scale

### Background Polling
- Configurable interval (default 30 seconds)
- Prevents concurrent polls with flag
- Dictionary lookups for O(1) aircraft comparison

## Scalability Path

### Current Architecture (Small-Medium Scale)
- File-based event store: < 100,000 events
- In-memory projections: < 10,000 entities
- Single process, single machine

### Future Enhancements for Scale
1. **Database Event Store**: SQL Server, PostgreSQL, EventStoreDB
2. **Distributed Processing**: Multiple consumers for projections
3. **Caching Layer**: Redis for read models
4. **Message Queue**: RabbitMQ, Azure Service Bus for events
5. **Horizontal Scaling**: Multiple instances with load balancer

## Testing Strategy

### Unit Tests
- Aggregate business logic
- Command validation
- Event application to projections
- Individual service methods

### Integration Tests
- Command handler full flow
- Event store persistence
- Projection rebuilding
- Repository operations

### Test Infrastructure
- `TestHelpers.cs`: Factory methods for test events
- In-memory repositories for testing
- Mock external API calls

## Security Considerations

### Current State
- No authentication (single-user console app)
- File system permissions control access
- Local data storage only

### Future Enhancements
- User authentication and authorization
- Encrypted event storage
- API key management for external services
- Audit logging with user attribution

## Documentation References

- [Background Services](BACKGROUND_SERVICES.md)
- [Command Handlers](COMMAND_HANDLERS.md)
- [Dependency Injection](DEPENDENCY_INJECTION.md)
- [Validation Framework](VALIDATION.md)
- [Queries](QUERIES.md)
- [Projections](PROJECTIONS.md)
- [Event Catalog](EVENT_CATALOG.md)

## Technology Stack

- **.NET 10.0**: Framework
- **Microsoft.Extensions.Hosting**: Background services
- **Microsoft.Extensions.DependencyInjection**: IOC container
- **Microsoft.Extensions.Http**: HTTP client factory
- **System.Text.Json**: JSON serialization
- **adsb.fi API**: External aircraft data source

## Design Patterns Used

1. **Event Sourcing**: All state changes captured as events
2. **CQRS**: Separate read and write models
3. **Repository**: Abstract data access
4. **Aggregate**: Transactional consistency boundaries
5. **Dependency Injection**: Loose coupling
6. **Observer**: Event dispatcher notifies projections
7. **Strategy**: Different projection strategies for different read models
8. **Factory**: Test helpers and entity creation
9. **Singleton**: Shared service instances
10. **Command**: Encapsulated requests

## Architectural Decisions

### Why Event Sourcing?
- Complete audit trail of all changes
- Temporal queries (state at any point in time)
- Event replay for debugging and analysis
- Foundation for future features (notifications, analytics)

### Why CQRS?
- Optimized read models for queries
- Independent scaling of reads and writes
- Flexibility to add new projections
- Clear separation of concerns

### Why File-Based Storage?
- Simple deployment (no database required)
- Easy backup and restore
- Human-readable for debugging
- Sufficient for current scale
- Easy migration path to database later

### Why Console Application?
- Rapid development and testing
- Low resource requirements
- Background services without web server overhead
- Foundation for future GUI or web interface

## Future Architecture Evolution

### Short Term
- Add more projections (snapshots, statistics)
- Implement event versioning and upcasting
- Add integration tests for full workflows
- Improve logging and monitoring

### Medium Term
- Web API for remote access
- Real-time notifications (SignalR)
- Advanced queries and filtering
- Performance optimization and caching

### Long Term
- Microservices architecture
- Cloud deployment (Azure, AWS)
- Distributed event store
- Advanced analytics and machine learning
- Mobile application support

---

*Last Updated: January 24, 2026*
