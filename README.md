# PlaneCrazy

A C# console application for tracking ADS-B aircraft data from adsb.fi using event-sourced architecture.

## Features

- **Aircraft Tracking**: Fetches real-time ADS-B aircraft data from adsb.fi
- **Favourites**: Save favourite aircraft, aircraft types, and airports
- **Comments**: Add comments to any aircraft, type, or airport
- **Event Sourcing**: All actions are stored as domain events in JSON files
- **Projections**: Read models are rebuilt from events for queries

## Architecture

The application follows clean architecture principles with three main projects:

### PlaneCrazy.Domain
Core domain logic including:
- **Entities**: Aircraft, AircraftType, Airport, Comment, Favourite
- **Events**: AircraftFavourited, TypeFavourited, AirportFavourited, CommentAdded, etc.
- **Interfaces**: Repository and service contracts

### PlaneCrazy.Infrastructure
Infrastructure implementations:
- **EventStore**: JSON file-based event store for event sourcing
- **Repositories**: JSON file-based repositories for entities
- **Services**: ADS-B data service for fetching aircraft data from adsb.fi
- **Projections**: Build read models from domain events

### PlaneCrazy.Console
Console user interface with menu-driven navigation.

## Data Storage

All data is stored as JSON files in the user's Documents folder under `Documents/PlaneCrazy/`:

- `EventStore/` - Domain events (immutable event log)
- `Repositories/` - Projected read models (aircraft.json, favourites.json, comments.json)

## Building and Running

```bash
# Build the solution
dotnet build

# Run the application
cd src/PlaneCrazy.Console
dotnet run
```

## Usage

1. **Fetch and View Aircraft** - Retrieves current aircraft data from adsb.fi
2. **Manage Favourites** - Add/remove/view favourite aircraft, types, and airports
3. **Manage Comments** - Add and view comments on entities
4. **View Event History** - Browse the complete event log

## Technology Stack

- .NET 10.0
- System.Text.Json for JSON serialization
- Event sourcing pattern
- Repository pattern
- Clean architecture

