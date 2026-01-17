# PlaneCrazy

A repository to build an app to handle ADS-B data, and learn some new architectural patterns.

## Features

### JSON File-Based Event Store

This project implements a JSON file-based event store that saves each event as a separate file under the directory structure `Events/{EntityType}/{EntityId}/`.

#### Key Features

- **Event Sourcing**: Store domain events as immutable, append-only records
- **File-Based Persistence**: Each event is saved as a separate JSON file
- **Hierarchical Organization**: Events are organized by entity type and entity ID
- **Sequential Numbering**: Events are numbered sequentially (001.json, 002.json, etc.)
- **Type Safety**: Strongly-typed events with base classes and interfaces
- **Async Support**: Full async/await support for I/O operations

#### Architecture

The event store consists of the following components:

- **IEvent**: Interface defining the contract for all events
- **EventBase**: Abstract base class for implementing domain events
- **IEventStore**: Interface defining event store operations
- **JsonFileEventStore**: JSON file-based implementation of the event store

#### Domain Events

The library includes sample domain events for aircraft tracking:

- **AircraftDetectedEvent**: Raised when a new aircraft is detected
- **PositionUpdatedEvent**: Raised when an aircraft's position is updated
- **SquawkChangedEvent**: Raised when an aircraft's squawk code changes

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later

### Building the Project

```bash
# Clone the repository
git clone https://github.com/groversaurus/PlaneCrazy.git
cd PlaneCrazy

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Running the Demo

```bash
dotnet run --project src/PlaneCrazy.Demo/PlaneCrazy.Demo.csproj
```

The demo application will:
1. Create a JsonFileEventStore instance
2. Simulate aircraft tracking events
3. Save events to the file system
4. Retrieve and display events
5. Show the resulting file structure

### Usage Example

```csharp
using PlaneCrazy.EventStore;
using PlaneCrazy.EventStore.Events;

// Create an event store
var eventStore = new JsonFileEventStore("./Events");

// Create and save an event
var aircraftDetected = new AircraftDetectedEvent
{
    EntityType = "Aircraft",
    EntityId = "ABC123",
    IcaoAddress = "ABC123",
    Callsign = "UAL123",
    Latitude = 40.7128,
    Longitude = -74.0060,
    Altitude = 35000
};

await eventStore.SaveEventAsync(aircraftDetected);

// Retrieve events for a specific entity
var events = await eventStore.GetEventsAsync("Aircraft", "ABC123");

// Retrieve all events for an entity type
var allAircraftEvents = await eventStore.GetEventsByEntityTypeAsync("Aircraft");
```

### File Structure

Events are stored in the following directory structure:

```
Events/
└── Aircraft/
    ├── ABC123/
    │   ├── 001.json
    │   ├── 002.json
    │   └── 003.json
    └── XYZ789/
        └── 001.json
```

Each JSON file contains the complete event data:

```json
{
  "entityId": "ABC123",
  "entityType": "Aircraft",
  "eventType": "AircraftDetectedEvent",
  "timestamp": "2026-01-17T20:38:45.919348Z",
  "data": {
    "icaoAddress": "ABC123",
    "callsign": "UAL123",
    "latitude": 40.7128,
    "longitude": -74.006,
    "altitude": 35000,
    "entityId": "ABC123",
    "entityType": "Aircraft",
    "timestamp": "2026-01-17T20:38:45.919348Z",
    "eventType": "AircraftDetectedEvent"
  }
}
```

## Project Structure

```
PlaneCrazy/
├── src/
│   ├── PlaneCrazy.EventStore/       # Event store library
│   │   ├── IEvent.cs                # Event interface
│   │   ├── EventBase.cs             # Base class for events
│   │   ├── IEventStore.cs           # Event store interface
│   │   ├── JsonFileEventStore.cs    # JSON file-based implementation
│   │   └── Events/                  # Domain events
│   │       ├── AircraftDetectedEvent.cs
│   │       ├── PositionUpdatedEvent.cs
│   │       └── SquawkChangedEvent.cs
│   └── PlaneCrazy.Demo/             # Console demo application
│       └── Program.cs
├── tests/
│   └── PlaneCrazy.EventStore.Tests/ # Unit tests
│       └── JsonFileEventStoreTests.cs
└── PlaneCrazy.sln                   # Solution file
```

## Testing

The project includes comprehensive unit tests covering:

- Directory and file creation
- Sequential numbering of events
- Event retrieval by entity and entity type
- Error handling and validation
- Multiple entities of the same type
- JSON serialization and deserialization

Run all tests:

```bash
dotnet test
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

