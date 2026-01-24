# PlaneCrazy Event Catalog

## Overview

This document catalogs all domain events in the PlaneCrazy system. Events are the source of truth in our event-sourced architecture, representing facts that have occurred in the system.

## Event Naming Convention

Events are named in **past tense** to indicate something that has already happened:
- ✅ `CommentAdded` (correct)
- ❌ `AddComment` (incorrect - sounds like command)

## Event Categories

1. **Favourite Events** - Favouriting/unfavouriting entities
2. **Comment Events** - Adding, editing, and deleting comments
3. **Aircraft Tracking Events** - Aircraft position and identity changes

## Event Schema Reference

### Base Event Properties

All events inherit from `DomainEvent`:

```csharp
public abstract class DomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
```

---

## Favourite Events

### AircraftFavourited

**When**: User adds an aircraft to favourites

**Properties**:
```csharp
public class AircraftFavourited : DomainEvent
{
    public required string Icao24 { get; set; }
    public string? Registration { get; set; }
    public string? TypeCode { get; set; }
    public override string EventType => nameof(AircraftFavourited);
}
```

**Validation Rules**:
- `Icao24`: Required, 6 hexadecimal characters
- `Registration`: Optional, 1-10 alphanumeric with hyphens
- `TypeCode`: Optional, 2-10 alphanumeric characters

**Example JSON**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "occurredAt": "2026-01-24T14:30:22.456Z",
  "eventType": "AircraftFavourited",
  "icao24": "ABC123",
  "registration": "N12345",
  "typeCode": "B738"
}
```

**Projections Affected**:
- `FavouriteProjection` - Adds aircraft to favourites

**Business Rules**:
- Aircraft can be favourited multiple times (idempotent at projection level)
- Unfavouriting and re-favouriting creates new favourite

---

### AircraftUnfavourited

**When**: User removes an aircraft from favourites

**Properties**:
```csharp
public class AircraftUnfavourited : DomainEvent
{
    public required string Icao24 { get; set; }
    public override string EventType => nameof(AircraftUnfavourited);
}
```

**Validation Rules**:
- `Icao24`: Required, 6 hexadecimal characters

**Example JSON**:
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "occurredAt": "2026-01-24T15:45:10.123Z",
  "eventType": "AircraftUnfavourited",
  "icao24": "ABC123"
}
```

**Projections Affected**:
- `FavouriteProjection` - Removes aircraft from favourites

---

### TypeFavourited

**When**: User adds an aircraft type to favourites

**Properties**:
```csharp
public class TypeFavourited : DomainEvent
{
    public required string TypeCode { get; set; }
    public string? TypeName { get; set; }
    public override string EventType => nameof(TypeFavourited);
}
```

**Validation Rules**:
- `TypeCode`: Required, 2-10 alphanumeric characters
- `TypeName`: Optional, max 200 characters

**Example JSON**:
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "occurredAt": "2026-01-24T16:20:00.789Z",
  "eventType": "TypeFavourited",
  "typeCode": "B738",
  "typeName": "Boeing 737-800"
}
```

**Projections Affected**:
- `FavouriteProjection` - Adds type to favourites

**Common Type Codes**:
- `B738` - Boeing 737-800
- `A320` - Airbus A320
- `B77W` - Boeing 777-300ER
- `A388` - Airbus A380-800

---

### TypeUnfavourited

**When**: User removes an aircraft type from favourites

**Properties**:
```csharp
public class TypeUnfavourited : DomainEvent
{
    public required string TypeCode { get; set; }
    public override string EventType => nameof(TypeUnfavourited);
}
```

**Validation Rules**:
- `TypeCode`: Required, 2-10 alphanumeric characters

**Example JSON**:
```json
{
  "id": "b2c3d4e5-f678-9012-3456-789abcdef012",
  "occurredAt": "2026-01-24T17:10:30.456Z",
  "eventType": "TypeUnfavourited",
  "typeCode": "B738"
}
```

**Projections Affected**:
- `FavouriteProjection` - Removes type from favourites

---

### AirportFavourited

**When**: User adds an airport to favourites

**Properties**:
```csharp
public class AirportFavourited : DomainEvent
{
    public required string IcaoCode { get; set; }
    public string? Name { get; set; }
    public override string EventType => nameof(AirportFavourited);
}
```

**Validation Rules**:
- `IcaoCode`: Required, 4 uppercase letters
- `Name`: Optional, max 200 characters

**Example JSON**:
```json
{
  "id": "c3d4e5f6-7890-1234-5678-90abcdef1234",
  "occurredAt": "2026-01-24T18:00:00.000Z",
  "eventType": "AirportFavourited",
  "icaoCode": "KJFK",
  "name": "John F. Kennedy International Airport"
}
```

**Projections Affected**:
- `FavouriteProjection` - Adds airport to favourites

**Common ICAO Codes**:
- `KJFK` - New York JFK
- `EGLL` - London Heathrow
- `LFPG` - Paris Charles de Gaulle
- `EDDF` - Frankfurt

---

### AirportUnfavourited

**When**: User removes an airport from favourites

**Properties**:
```csharp
public class AirportUnfavourited : DomainEvent
{
    public required string IcaoCode { get; set; }
    public override string EventType => nameof(AirportUnfavourited);
}
```

**Validation Rules**:
- `IcaoCode`: Required, 4 uppercase letters

**Example JSON**:
```json
{
  "id": "d4e5f678-9012-3456-7890-abcdef123456",
  "occurredAt": "2026-01-24T19:30:15.789Z",
  "eventType": "AirportUnfavourited",
  "icaoCode": "KJFK"
}
```

**Projections Affected**:
- `FavouriteProjection` - Removes airport from favourites

---

## Comment Events

### CommentAdded

**When**: User adds a comment to an entity

**Properties**:
```csharp
public class CommentAdded : DomainEvent
{
    public required string EntityType { get; set; }
    public required string EntityId { get; set; }
    public required string Text { get; set; }
    public string? User { get; set; }
    public override string EventType => nameof(CommentAdded);
}
```

**Validation Rules**:
- `EntityType`: Required, must be "Aircraft", "Type", or "Airport"
- `EntityId`: Required, validated based on entity type
- `Text`: Required, 1-5000 characters
- `User`: Optional, max 100 characters

**Example JSON**:
```json
{
  "id": "e5f67890-1234-5678-90ab-cdef12345678",
  "occurredAt": "2026-01-24T20:00:00.123Z",
  "eventType": "CommentAdded",
  "entityType": "Aircraft",
  "entityId": "ABC123",
  "text": "Spotted this beautiful aircraft at LAX today!",
  "user": "john.doe"
}
```

**Projections Affected**:
- `CommentProjection` - Creates new comment record

**Business Rules**:
- Comments can be added to any valid entity
- Multiple comments allowed per entity
- Comment ID generated automatically (event ID)

---

### CommentEdited

**When**: User edits an existing comment

**Properties**:
```csharp
public class CommentEdited : DomainEvent
{
    public required Guid CommentId { get; set; }
    public required string Text { get; set; }
    public string? User { get; set; }
    public override string EventType => nameof(CommentEdited);
}
```

**Validation Rules**:
- `CommentId`: Required, valid GUID
- `Text`: Required, 1-5000 characters
- `User`: Optional, max 100 characters

**Example JSON**:
```json
{
  "id": "f6789012-3456-7890-abcd-ef1234567890",
  "occurredAt": "2026-01-24T20:15:30.456Z",
  "eventType": "CommentEdited",
  "commentId": "e5f67890-1234-5678-90ab-cdef12345678",
  "text": "Spotted this beautiful aircraft at LAX today! Update: It was a B738.",
  "user": "john.doe"
}
```

**Projections Affected**:
- `CommentProjection` - Updates comment text and metadata

**Business Rules**:
- Comment must exist
- Comment must not be deleted
- Edit history tracked via events

---

### CommentDeleted

**When**: User deletes a comment

**Properties**:
```csharp
public class CommentDeleted : DomainEvent
{
    public required Guid CommentId { get; set; }
    public override string EventType => nameof(CommentDeleted);
}
```

**Validation Rules**:
- `CommentId`: Required, valid GUID

**Example JSON**:
```json
{
  "id": "78901234-5678-90ab-cdef-123456789012",
  "occurredAt": "2026-01-24T21:00:00.789Z",
  "eventType": "CommentDeleted",
  "commentId": "e5f67890-1234-5678-90ab-cdef12345678"
}
```

**Projections Affected**:
- `CommentProjection` - Marks comment as deleted (soft delete)

**Business Rules**:
- Comment must exist
- Comment must not already be deleted
- Soft delete (comment not removed from database)
- Deleted comments not shown in UI

---

## Aircraft Tracking Events

### AircraftFirstSeen

**When**: Background poller detects new aircraft

**Properties**:
```csharp
public class AircraftFirstSeen : DomainEvent
{
    public required string Icao24 { get; set; }
    public DateTime FirstSeenAt { get; set; }
    public double? InitialLatitude { get; set; }
    public double? InitialLongitude { get; set; }
    public override string EventType => nameof(AircraftFirstSeen);
}
```

**Validation Rules**:
- `Icao24`: Required, 6 hexadecimal characters
- `FirstSeenAt`: Required
- `InitialLatitude`: Optional, -90 to 90
- `InitialLongitude`: Optional, -180 to 180

**Example JSON**:
```json
{
  "id": "89012345-6789-0abc-def1-234567890123",
  "occurredAt": "2026-01-24T22:00:00.000Z",
  "eventType": "AircraftFirstSeen",
  "icao24": "ABC123",
  "firstSeenAt": "2026-01-24T22:00:00.000Z",
  "initialLatitude": 51.5074,
  "initialLongitude": -0.1278
}
```

**Projections Affected**:
- `AircraftStateProjection` - Creates aircraft record

**Generated By**: `BackgroundAdsBPoller` service

---

### AircraftPositionUpdated

**When**: Aircraft position/altitude/speed changes

**Properties**:
```csharp
public class AircraftPositionUpdated : DomainEvent
{
    public required string Icao24 { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Altitude { get; set; }
    public double? Velocity { get; set; }
    public double? Track { get; set; }
    public double? VerticalRate { get; set; }
    public bool? OnGround { get; set; }
    public DateTime Timestamp { get; set; }
    public override string EventType => nameof(AircraftPositionUpdated);
}
```

**Validation Rules**:
- `Icao24`: Required, 6 hexadecimal characters
- Numeric values: Optional, within realistic ranges
- `Timestamp`: Required

**Example JSON**:
```json
{
  "id": "90123456-789a-bcde-f123-4567890abcde",
  "occurredAt": "2026-01-24T22:05:00.000Z",
  "eventType": "AircraftPositionUpdated",
  "icao24": "ABC123",
  "latitude": 51.5100,
  "longitude": -0.1300,
  "altitude": 3500,
  "velocity": 250.5,
  "track": 270.0,
  "verticalRate": 1200,
  "onGround": false,
  "timestamp": "2026-01-24T22:05:00.000Z"
}
```

**Projections Affected**:
- `AircraftStateProjection` - Updates aircraft position
- `SnapshotProjection` - Used for historical queries

**Generated By**: `BackgroundAdsBPoller` service

**Frequency**: Every polling cycle when position changes (typically every 30 seconds)

---

### AircraftIdentityUpdated

**When**: Aircraft callsign, registration, or type changes

**Properties**:
```csharp
public class AircraftIdentityUpdated : DomainEvent
{
    public required string Icao24 { get; set; }
    public string? Registration { get; set; }
    public string? TypeCode { get; set; }
    public string? Callsign { get; set; }
    public string? Squawk { get; set; }
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public DateTime Timestamp { get; set; }
    public override string EventType => nameof(AircraftIdentityUpdated);
}
```

**Validation Rules**:
- `Icao24`: Required, 6 hexadecimal characters
- `Registration`: Optional, 1-10 alphanumeric with hyphens
- `TypeCode`: Optional, 2-10 alphanumeric
- Other fields: Optional with sensible max lengths

**Example JSON**:
```json
{
  "id": "0123456789abcdef0123456789abcdef",
  "occurredAt": "2026-01-24T22:10:00.000Z",
  "eventType": "AircraftIdentityUpdated",
  "icao24": "ABC123",
  "registration": "N12345",
  "typeCode": "B738",
  "callsign": "UAL123",
  "squawk": "1200",
  "origin": "KJFK",
  "destination": "EGLL",
  "timestamp": "2026-01-24T22:10:00.000Z"
}
```

**Projections Affected**:
- `AircraftStateProjection` - Updates aircraft identity
- `SnapshotProjection` - Used for historical queries

**Generated By**: `BackgroundAdsBPoller` service

**Frequency**: When identity information changes or becomes available

---

### AircraftLastSeen

**When**: Aircraft not detected for 5 minutes (configurable)

**Properties**:
```csharp
public class AircraftLastSeen : DomainEvent
{
    public required string Icao24 { get; set; }
    public DateTime LastSeenAt { get; set; }
    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }
    public double? LastAltitude { get; set; }
    public override string EventType => nameof(AircraftLastSeen);
}
```

**Validation Rules**:
- `Icao24`: Required, 6 hexadecimal characters
- `LastSeenAt`: Required
- Position fields: Optional

**Example JSON**:
```json
{
  "id": "123456789abcdef0123456789abcdef0",
  "occurredAt": "2026-01-24T22:35:00.000Z",
  "eventType": "AircraftLastSeen",
  "icao24": "ABC123",
  "lastSeenAt": "2026-01-24T22:30:00.000Z",
  "lastLatitude": 51.5200,
  "lastLongitude": -0.1400,
  "lastAltitude": 0
}
```

**Projections Affected**:
- `AircraftStateProjection` - Updates last seen timestamp
- `SnapshotProjection` - Records final known position

**Generated By**: `BackgroundAdsBPoller` service

**Frequency**: Once per aircraft after timeout period

---

## Event Flow Patterns

### User-Initiated Events

```
User Action → Command → Command Handler → Event → Event Store → Projections
```

**Examples**: Favouriting, commenting, editing

### System-Generated Events

```
Background Service → Data Comparison → Event → Event Store → Projections
```

**Examples**: Aircraft tracking events

## Event Versioning

### Current Version

All events are **Version 1.0** (implicit)

### Future Versioning Strategy

When event schemas need to change:

1. **Add new optional fields**: Backward compatible
2. **Deprecate fields**: Mark as obsolete, keep for compatibility
3. **Breaking changes**: Create new event type with version suffix

**Example**:
```csharp
// V1 (current)
public class CommentAdded : DomainEvent { }

// V2 (future)
public class CommentAddedV2 : DomainEvent
{
    // New schema with breaking changes
}
```

## Event Size Considerations

### Typical Event Sizes

| Event Type | Approximate Size | Notes |
|------------|-----------------|-------|
| AircraftFavourited | 300 bytes | Small, metadata only |
| CommentAdded | 500-5500 bytes | Varies with text length |
| AircraftPositionUpdated | 400 bytes | Fixed-size numeric data |
| AircraftIdentityUpdated | 350 bytes | String fields, moderate size |

### Large Events

Comments can be up to 5000 characters:
- Average: 100-500 bytes
- Maximum: ~5.5 KB with metadata

**Recommendation**: No pagination needed for current event sizes

## Event Retention

### Current Policy

- **All events retained indefinitely**
- No automatic deletion
- Event store grows continuously

### File Structure

```
Documents/PlaneCrazy/Events/
├── 20260124143022456_3fa85f64-5717-4562-b3fc-2c963f66afa6.json
├── 20260124143023789_7c9e6679-7425-40de-944b-e07fc1f90ae7.json
└── ...
```

**Filename Format**: `{timestamp}_{eventId}.json`

### Future Considerations

- Archive old events (> 1 year)
- Compress archived events
- Snapshot-based rebuilds to reduce replay time

## Event Statistics

### Metrics to Track

```csharp
public class EventStatistics
{
    public int TotalEvents { get; set; }
    public Dictionary<string, int> EventCountsByType { get; set; }
    public DateTime OldestEvent { get; set; }
    public DateTime NewestEvent { get; set; }
    public long TotalStorageBytes { get; set; }
}
```

### Example Query

```csharp
var events = await eventStore.GetAllAsync();
var stats = new
{
    Total = events.Count(),
    ByType = events.GroupBy(e => e.EventType)
                   .ToDictionary(g => g.Key, g => g.Count()),
    FavouriteEvents = events.Count(e => e is AircraftFavourited or TypeFavourited or AirportFavourited),
    CommentEvents = events.Count(e => e is CommentAdded or CommentEdited or CommentDeleted),
    TrackingEvents = events.Count(e => e is AircraftFirstSeen or AircraftPositionUpdated)
};
```

## Best Practices

1. **Immutable Events**: Never modify events after creation
2. **Past Tense**: Always name events in past tense
3. **Rich Data**: Include all relevant information in event
4. **Timestamps**: Always include `OccurredAt`
5. **IDs**: Use GUIDs for event IDs
6. **Validation**: Validate before persisting
7. **Small Events**: Keep events focused and small
8. **Metadata**: Include contextual information
9. **Documentation**: Document business meaning
10. **Testing**: Test event serialization/deserialization

## Common Queries

### Get all events for an entity

```csharp
var events = await eventStore.GetAllAsync();
var aircraftEvents = events.Where(e => 
    e is AircraftFavourited af && af.Icao24 == "ABC123" ||
    e is CommentAdded ca && ca.EntityType == "Aircraft" && ca.EntityId == "ABC123");
```

### Get events by type

```csharp
var commentEvents = await eventStore.GetByTypeAsync("CommentAdded");
```

### Get events in time range

```csharp
var recentEvents = await eventStore.ReadEventsAsync(
    fromTimestamp: DateTime.UtcNow.AddHours(-24),
    toTimestamp: DateTime.UtcNow);
```

---

*Last Updated: January 24, 2026*
