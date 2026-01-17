# PlaneCrazy

A .NET library for handling ADS-B (Automatic Dependent Surveillance-Broadcast) data from the adsb.fi API, while exploring modern architectural patterns.

## Features

- **ApiClient**: Base HTTP client for making API requests with JSON serialization
- **AdsBFiRepository**: Repository pattern implementation for accessing adsb.fi endpoints
- **Strongly Typed Models**: Fully typed models for aircraft data with JSON serialization
- **Comprehensive Testing**: Full test coverage with xUnit, Moq, and FluentAssertions

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Usage

### Basic Usage

```csharp
using PlaneCrazy.Core.Repositories;
using PlaneCrazy.Core.Services;

// Create the HTTP client and API client
var httpClient = new HttpClient();
var apiClient = new ApiClient(httpClient);

// Create the repository
var repository = new AdsBFiRepository(apiClient);

// Get all aircraft
var allAircraft = await repository.GetAllAircraftAsync();
Console.WriteLine($"Found {allAircraft?.Aircraft.Count} aircraft");

// Get aircraft near a location (e.g., Helsinki-Vantaa Airport)
var nearbyAircraft = await repository.GetAircraftByLocationAsync(
    latitude: 60.3172,
    longitude: 24.9633,
    radiusNm: 50.0
);

// Get a specific aircraft by its hex code
var aircraft = await repository.GetAircraftByHexAsync("ABC123");
if (aircraft != null)
{
    Console.WriteLine($"Flight: {aircraft.Flight}, Altitude: {aircraft.Altitude}ft");
}
```

### Using Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using PlaneCrazy.Core.Repositories;
using PlaneCrazy.Core.Services;

var services = new ServiceCollection();

// Register HTTP client and dependencies
services.AddHttpClient<ApiClient>();
services.AddScoped<IAdsBFiRepository, AdsBFiRepository>();

var serviceProvider = services.BuildServiceProvider();

// Use the repository
var repository = serviceProvider.GetRequiredService<IAdsBFiRepository>();
var aircraft = await repository.GetAllAircraftAsync();
```

## API Endpoints

The `IAdsBFiRepository` interface provides access to the following endpoints:

### GetAllAircraftAsync()
Retrieves all aircraft currently being tracked by the adsb.fi network.

### GetAircraftByLocationAsync(latitude, longitude, radiusNm)
Retrieves aircraft within a specific geographic area defined by:
- `latitude`: Center latitude (-90 to 90)
- `longitude`: Center longitude (-180 to 180)
- `radiusNm`: Search radius in nautical miles (> 0)

### GetAircraftByHexAsync(hex)
Retrieves a specific aircraft by its ICAO 24-bit address (hex code).

## Models

### Aircraft
Represents an individual aircraft with properties including:
- `Hex`: ICAO 24-bit aircraft address
- `Flight`: Callsign/flight number
- `Latitude`/`Longitude`: Current position
- `Altitude`: Altitude in feet
- `GroundSpeed`: Speed in knots
- `Track`: Heading in degrees
- And many more fields...

### AircraftResponse
Container for API responses with:
- `Now`: Current timestamp
- `Messages`: Number of messages processed
- `Aircraft`: List of aircraft

## Project Structure

```
PlaneCrazy/
├── src/
│   └── PlaneCrazy.Core/
│       ├── Models/           # Data models
│       ├── Repositories/     # Repository implementations
│       └── Services/         # Service layer (ApiClient)
└── tests/
    └── PlaneCrazy.Core.Tests/
        ├── Models/           # Model tests
        ├── Repositories/     # Repository tests
        └── Services/         # Service tests
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
