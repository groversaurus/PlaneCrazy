# PlaneCrazy

An application to handle ADS-B data, built to learn and demonstrate modern architectural patterns.

## Project Structure

This solution follows a clean architecture pattern with clear separation of concerns:

```
PlaneCrazy/
├── src/
│   ├── PlaneCrazy.Core/          # Core business logic and domain models
│   ├── PlaneCrazy.Console/        # Spectre.Console CLI application
│   └── PlaneCrazy.Blazor/         # Blazor web UI
└── tests/
    └── PlaneCrazy.Core.Tests/     # Unit tests for core functionality
```

## Technologies

- **.NET 9.0** - Modern .NET platform
- **Spectre.Console** - Rich console UI framework
- **Blazor Server** - Interactive web UI
- **xUnit** - Testing framework
- **C# 13** - Latest language features

## Architecture

### PlaneCrazy.Core
Contains the core business logic, domain models, and service interfaces. This layer is:
- Independent of UI frameworks
- Fully testable
- Reusable across different UIs

Key components:
- `AircraftData` - Domain model representing ADS-B aircraft data
- `IAircraftDataService` - Service interface for managing aircraft data
- `InMemoryAircraftDataService` - In-memory implementation of the service

### PlaneCrazy.Console
A rich console application using Spectre.Console that provides:
- Interactive menu-driven interface
- Aircraft data viewing and management
- Colorful, styled console output

### PlaneCrazy.Blazor
A Blazor Server web application that provides:
- Web-based UI for aircraft tracking
- Real-time data updates
- Responsive design with Bootstrap

### PlaneCrazy.Core.Tests
Comprehensive unit tests using xUnit covering:
- Domain model behavior
- Service implementations
- Edge cases and error conditions

## Building and Running

### Prerequisites
- .NET 9.0 SDK or later

### Build
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Run Console Application
```bash
dotnet run --project src/PlaneCrazy.Console
```

### Run Blazor Application
```bash
dotnet run --project src/PlaneCrazy.Blazor
```

Then navigate to the URL shown in the console output (typically `https://localhost:5001` or `http://localhost:5000`).

## Features

### Current Features
- In-memory aircraft data storage
- View all tracked aircraft
- Search aircraft by ICAO address
- Add sample aircraft data
- Console UI with Spectre.Console
- Web UI with Blazor

### Future Enhancements
- Real ADS-B data integration
- Persistent data storage
- Real-time updates
- Map visualization
- Advanced filtering and search
- Historical data tracking

## Testing

The solution uses xUnit for testing, which is Microsoft's recommended testing framework. Tests are located in the `tests/` directory and cover:

- **Model Tests**: Validate domain model behavior
- **Service Tests**: Test service implementations including edge cases

Run all tests:
```bash
dotnet test
```

Run tests with detailed output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Development

This project demonstrates:
- Clean architecture principles
- Dependency injection
- Interface-based design
- Separation of concerns
- Test-driven development
- Multiple UI paradigms (Console and Web)

## License

See LICENSE file for details.

