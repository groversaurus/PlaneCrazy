# PlaneCrazy Query Results Documentation

## Overview

This document catalogs all query result DTOs (Data Transfer Objects) and view models used in the PlaneCrazy read side (CQRS queries).

## Query Architecture

### Query Flow

```
User Request
    ↓
Query Service (IAircraftQueryService, ICommentQueryService, etc.)
    ↓
Projection (Read-optimized data store)
    ↓
Query Result DTO
    ↓
User (Console output, UI, API response, etc.)
```

### Why DTOs?

**Benefits**:
- Decouple domain models from presentation
- Optimize for specific query needs
- Avoid exposing internal structure
- Easy to serialize/deserialize
- Versioning without breaking changes

---

## Aircraft Query Results

### AircraftDto

**Purpose**: Represents current aircraft state

**Properties**:
```csharp
public class AircraftDto
{
    public required string Icao24 { get; set; }
    public string? Registration { get; set; }
    public string? TypeCode { get; set; }
    public string? Callsign { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Altitude { get; set; }
    public double? Velocity { get; set; }
    public double? Track { get; set; }
    public double? VerticalRate { get; set; }
    public bool OnGround { get; set; }
    public string? Squawk { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
}
```

**Example JSON**:
```json
{
  "icao24": "ABC123",
  "registration": "N12345",
  "typeCode": "B738",
  "callsign": "UAL123",
  "latitude": 40.7128,
  "longitude": -74.0060,
  "altitude": 35000,
  "velocity": 450.5,
  "track": 270.0,
  "verticalRate": 0,
  "onGround": false,
  "squawk": "1200",
  "firstSeen": "2026-01-24T10:00:00Z",
  "lastSeen": "2026-01-24T14:30:22Z"
}
```

**Usage**:
```csharp
var aircraft = await _aircraftQueryService.GetByIcao24Async("ABC123");
if (aircraft != null)
{
    Console.WriteLine($"Aircraft {aircraft.Registration} at {aircraft.Altitude}ft");
}
```

---

### AircraftListDto

**Purpose**: List of aircraft with pagination metadata

**Properties**:
```csharp
public class AircraftListDto
{
    public IEnumerable<AircraftDto> Aircraft { get; set; } = Enumerable.Empty<AircraftDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

**Example JSON**:
```json
{
  "aircraft": [
    {
      "icao24": "ABC123",
      "registration": "N12345",
      ...
    },
    {
      "icao24": "DEF456",
      "registration": "N67890",
      ...
    }
  ],
  "totalCount": 1523,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 31
}
```

**Usage**:
```csharp
var result = await _aircraftQueryService.GetAllAsync(page: 1, pageSize: 50);
Console.WriteLine($"Showing {result.Aircraft.Count()} of {result.TotalCount} aircraft");
```

---

### AircraftSummaryDto

**Purpose**: Lightweight aircraft information for lists

**Properties**:
```csharp
public class AircraftSummaryDto
{
    public required string Icao24 { get; set; }
    public string? Registration { get; set; }
    public string? TypeCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastSeen { get; set; }
}
```

**Example JSON**:
```json
{
  "icao24": "ABC123",
  "registration": "N12345",
  "typeCode": "B738",
  "isActive": true,
  "lastSeen": "2026-01-24T14:30:00Z"
}
```

**Usage**:
```csharp
var summaries = await _aircraftQueryService.GetActiveSummariesAsync();
foreach (var summary in summaries)
{
    Console.WriteLine($"{summary.Registration} ({summary.TypeCode})");
}
```

---

### AircraftHistoryDto

**Purpose**: Historical position data for aircraft

**Properties**:
```csharp
public class AircraftHistoryDto
{
    public required string Icao24 { get; set; }
    public IEnumerable<PositionSnapshot> Positions { get; set; } = Enumerable.Empty<PositionSnapshot>();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class PositionSnapshot
{
    public DateTime Timestamp { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public double? Velocity { get; set; }
}
```

**Example JSON**:
```json
{
  "icao24": "ABC123",
  "startTime": "2026-01-24T10:00:00Z",
  "endTime": "2026-01-24T14:00:00Z",
  "positions": [
    {
      "timestamp": "2026-01-24T10:00:00Z",
      "latitude": 40.6413,
      "longitude": -73.7781,
      "altitude": 500,
      "velocity": 150.0
    },
    {
      "timestamp": "2026-01-24T10:05:00Z",
      "latitude": 40.7128,
      "longitude": -74.0060,
      "altitude": 5000,
      "velocity": 250.0
    }
  ]
}
```

**Usage**:
```csharp
var history = await _aircraftQueryService.GetHistoryAsync(
    "ABC123", 
    DateTime.UtcNow.AddHours(-4),
    DateTime.UtcNow);

Console.WriteLine($"Flight path with {history.Positions.Count()} positions");
```

---

## Comment Query Results

### CommentDto

**Purpose**: Represents a comment on an entity

**Properties**:
```csharp
public class CommentDto
{
    public Guid Id { get; set; }
    public required string EntityType { get; set; }
    public required string EntityId { get; set; }
    public required string Text { get; set; }
    public string? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastEditedAt { get; set; }
    public bool IsDeleted { get; set; }
}
```

**Example JSON**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "entityType": "Aircraft",
  "entityId": "ABC123",
  "text": "Spotted this beautiful 737 at LAX!",
  "user": "john.doe",
  "createdAt": "2026-01-24T14:30:22Z",
  "lastEditedAt": null,
  "isDeleted": false
}
```

**Usage**:
```csharp
var comment = await _commentQueryService.GetByIdAsync(commentId);
if (comment != null && !comment.IsDeleted)
{
    Console.WriteLine($"{comment.User}: {comment.Text}");
}
```

---

### CommentListDto

**Purpose**: List of comments with pagination

**Properties**:
```csharp
public class CommentListDto
{
    public IEnumerable<CommentDto> Comments { get; set; } = Enumerable.Empty<CommentDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

**Example JSON**:
```json
{
  "comments": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "entityType": "Aircraft",
      "entityId": "ABC123",
      "text": "Great aircraft!",
      "user": "john.doe",
      "createdAt": "2026-01-24T14:30:00Z",
      "lastEditedAt": null,
      "isDeleted": false
    }
  ],
  "totalCount": 127,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 7
}
```

**Usage**:
```csharp
var comments = await _commentQueryService.GetByEntityAsync(
    "Aircraft", 
    "ABC123",
    page: 1,
    pageSize: 20);

Console.WriteLine($"{comments.TotalCount} comments on this aircraft");
```

---

### CommentSummaryDto

**Purpose**: Lightweight comment metadata

**Properties**:
```csharp
public class CommentSummaryDto
{
    public Guid Id { get; set; }
    public required string EntityType { get; set; }
    public required string EntityId { get; set; }
    public string TextPreview { get; set; } = string.Empty; // First 100 chars
    public string? User { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Example JSON**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "entityType": "Aircraft",
  "entityId": "ABC123",
  "textPreview": "Spotted this beautiful 737 at LAX! It was performing a touch-and-go landing when I...",
  "user": "john.doe",
  "createdAt": "2026-01-24T14:30:00Z"
}
```

---

## Favourite Query Results

### FavouriteDto

**Purpose**: Represents a favourited entity

**Properties**:
```csharp
public class FavouriteDto
{
    public Guid Id { get; set; }
    public required string FavouriteType { get; set; } // "Aircraft", "Type", "Airport"
    public required string EntityId { get; set; }
    public DateTime FavouritedAt { get; set; }
    
    // Optional enriched data
    public string? DisplayName { get; set; }
    public string? AdditionalInfo { get; set; }
}
```

**Example JSON**:
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "favouriteType": "Aircraft",
  "entityId": "ABC123",
  "favouritedAt": "2026-01-20T10:00:00Z",
  "displayName": "N12345 (Boeing 737-800)",
  "additionalInfo": "Last seen 2 hours ago"
}
```

**Usage**:
```csharp
var favourite = await _favouriteQueryService.GetByIdAsync(favouriteId);
Console.WriteLine($"Favourited {favourite.DisplayName}");
```

---

### FavouriteListDto

**Purpose**: List of favourites grouped by type

**Properties**:
```csharp
public class FavouriteListDto
{
    public IEnumerable<FavouriteDto> Aircraft { get; set; } = Enumerable.Empty<FavouriteDto>();
    public IEnumerable<FavouriteDto> Types { get; set; } = Enumerable.Empty<FavouriteDto>();
    public IEnumerable<FavouriteDto> Airports { get; set; } = Enumerable.Empty<FavouriteDto>();
    public int TotalCount { get; set; }
}
```

**Example JSON**:
```json
{
  "aircraft": [
    {
      "id": "...",
      "favouriteType": "Aircraft",
      "entityId": "ABC123",
      "displayName": "N12345 (B738)"
    }
  ],
  "types": [
    {
      "id": "...",
      "favouriteType": "Type",
      "entityId": "B738",
      "displayName": "Boeing 737-800"
    }
  ],
  "airports": [
    {
      "id": "...",
      "favouriteType": "Airport",
      "entityId": "KJFK",
      "displayName": "New York JFK"
    }
  ],
  "totalCount": 15
}
```

**Usage**:
```csharp
var favourites = await _favouriteQueryService.GetAllAsync();
Console.WriteLine($"Aircraft: {favourites.Aircraft.Count()}");
Console.WriteLine($"Types: {favourites.Types.Count()}");
Console.WriteLine($"Airports: {favourites.Airports.Count()}");
```

---

## Statistics and Analytics

### SystemStatisticsDto

**Purpose**: Overall system statistics

**Properties**:
```csharp
public class SystemStatisticsDto
{
    public int TotalAircraft { get; set; }
    public int ActiveAircraft { get; set; }
    public int TotalComments { get; set; }
    public int TotalFavourites { get; set; }
    public int TotalEvents { get; set; }
    public DateTime FirstEventTime { get; set; }
    public DateTime LastEventTime { get; set; }
    public DateTime QueryTime { get; set; }
}
```

**Example JSON**:
```json
{
  "totalAircraft": 1523,
  "activeAircraft": 342,
  "totalComments": 89,
  "totalFavourites": 23,
  "totalEvents": 45821,
  "firstEventTime": "2026-01-01T00:00:00Z",
  "lastEventTime": "2026-01-24T14:30:22Z",
  "queryTime": "2026-01-24T14:30:30Z"
}
```

---

### AircraftStatisticsDto

**Purpose**: Statistics for specific aircraft

**Properties**:
```csharp
public class AircraftStatisticsDto
{
    public required string Icao24 { get; set; }
    public int TotalFlights { get; set; }
    public int TotalComments { get; set; }
    public bool IsFavourited { get; set; }
    public TimeSpan TotalTrackingTime { get; set; }
    public double AverageAltitude { get; set; }
    public double MaxAltitude { get; set; }
    public double AverageVelocity { get; set; }
    public double MaxVelocity { get; set; }
}
```

**Example JSON**:
```json
{
  "icao24": "ABC123",
  "totalFlights": 42,
  "totalComments": 5,
  "isFavourited": true,
  "totalTrackingTime": "PT15H30M",
  "averageAltitude": 35000,
  "maxAltitude": 41000,
  "averageVelocity": 450.5,
  "maxVelocity": 520.3
}
```

---

## Search Results

### SearchResultDto

**Purpose**: Generic search result across multiple entity types

**Properties**:
```csharp
public class SearchResultDto
{
    public string ResultType { get; set; } = string.Empty; // "Aircraft", "Comment", etc.
    public string EntityId { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public double Relevance { get; set; }
    public object? Data { get; set; } // Actual DTO (AircraftDto, CommentDto, etc.)
}
```

**Example JSON**:
```json
{
  "resultType": "Aircraft",
  "entityId": "ABC123",
  "displayText": "N12345 - Boeing 737-800",
  "snippet": "Boeing 737-800 with registration N12345, last seen at 35000ft",
  "relevance": 0.95,
  "data": {
    "icao24": "ABC123",
    "registration": "N12345",
    ...
  }
}
```

---

## DTO Mapping

### Mapping Domain Entities to DTOs

**Example**: Aircraft → AircraftDto

```csharp
public class AircraftMapper
{
    public static AircraftDto ToDto(Aircraft aircraft)
    {
        return new AircraftDto
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
            Squawk = aircraft.Squawk,
            FirstSeen = aircraft.FirstSeen,
            LastSeen = aircraft.LastSeen
        };
    }
    
    public static IEnumerable<AircraftDto> ToDtos(IEnumerable<Aircraft> aircraft)
    {
        return aircraft.Select(ToDto);
    }
}
```

---

## DTO Best Practices

### 1. Keep DTOs Simple

**Good**:
```csharp
public class CommentDto
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Bad**:
```csharp
public class CommentDto
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEventStore EventStore { get; set; } // ❌ Don't include services
    public void Save() { } // ❌ Don't include behavior
}
```

### 2. Use Nullable Types Appropriately

```csharp
public class AircraftDto
{
    public required string Icao24 { get; set; }  // Always present
    public string? Registration { get; set; }     // May be null
    public double? Altitude { get; set; }         // May be null
}
```

### 3. Include Metadata

```csharp
public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage => PageNumber * PageSize < TotalCount;
    public bool HasPreviousPage => PageNumber > 1;
}
```

### 4. Version DTOs

```csharp
// V1
public class CommentDtoV1
{
    public Guid Id { get; set; }
    public string Text { get; set; }
}

// V2 (with additional fields)
public class CommentDtoV2 : CommentDtoV1
{
    public string? Sentiment { get; set; }
    public double? SentimentScore { get; set; }
}
```

### 5. Document DTO Structure

Always include XML comments:

```csharp
/// <summary>
/// Represents an aircraft's current state and position.
/// </summary>
public class AircraftDto
{
    /// <summary>
    /// ICAO24 transponder address (unique identifier).
    /// Format: 6 hexadecimal characters (e.g., "ABC123").
    /// </summary>
    public required string Icao24 { get; set; }
    
    /// <summary>
    /// Aircraft altitude in feet above sea level.
    /// Null if altitude data is unavailable.
    /// </summary>
    public double? Altitude { get; set; }
}
```

---

## Future Enhancements

1. **GraphQL Support**: Allow clients to specify fields
2. **OData Support**: Advanced filtering and querying
3. **HAL/JSON:API**: Hypermedia-driven DTOs
4. **Localization**: Multi-language display text
5. **Rich Metadata**: Links, actions, permissions
6. **Real-time Updates**: WebSocket-compatible DTOs
7. **Partial Updates**: PATCH-style DTOs
8. **Computed Fields**: Dynamic calculated properties

---

*Last Updated: January 24, 2026*
