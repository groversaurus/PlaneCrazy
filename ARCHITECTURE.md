# Architecture Documentation

## Overview

PlaneCrazy follows a **Clean Architecture** pattern with clear separation of concerns. The solution is organized into distinct layers that promote testability, maintainability, and flexibility.

## Architectural Layers

### Core Layer (PlaneCrazy.Core)

The heart of the application containing business logic and domain models. This layer:
- Has no dependencies on UI or external frameworks
- Defines interfaces for services
- Contains domain models representing ADS-B aircraft data
- Is fully testable in isolation

**Key Responsibilities:**
- Domain model definitions
- Business logic
- Service contracts (interfaces)

### Application Layer (Console & Blazor)

Multiple presentation layers that depend on the Core:
- **PlaneCrazy.Console**: Rich terminal UI using Spectre.Console
- **PlaneCrazy.Blazor**: Web-based UI using Blazor Server

**Key Responsibilities:**
- User interaction
- Presentation logic
- Dependency injection configuration
- UI-specific formatting and display

### Test Layer (PlaneCrazy.Core.Tests)

Comprehensive test coverage using xUnit:
- Unit tests for domain models
- Unit tests for service implementations
- Integration tests for core functionality

## Design Patterns

### Dependency Injection

All projects use constructor-based dependency injection:
```csharp
public Application(IAircraftDataService aircraftDataService)
{
    _aircraftDataService = aircraftDataService;
}
```

Services are registered at application startup:
```csharp
services.AddSingleton<IAircraftDataService, InMemoryAircraftDataService>();
```

### Repository Pattern

The `IAircraftDataService` interface abstracts data access, making it easy to:
- Swap implementations (in-memory, database, API)
- Test with mocks
- Change persistence strategies without affecting consumers

### Interface Segregation

Each service has a focused, single-responsibility interface:
- `IAircraftDataService` - Aircraft data management
- Future: `IAdsbReceiver` - ADS-B data reception
- Future: `IWeatherService` - Weather data integration

## Testing Strategy

### Unit Tests

Test individual components in isolation:
- Model behavior
- Service implementations
- Business logic

### Test Framework: xUnit

We chose xUnit because:
- It's Microsoft's recommended testing framework
- Modern, extensible architecture
- Excellent integration with .NET tooling
- Strong community support
- Better than NUnit for new projects

xUnit features used:
- `[Fact]` - Individual test methods
- `[Theory]` - Parameterized tests (for future use)
- Constructor setup for test initialization
- Async test support

## Data Flow

```
User Input (Console/Blazor)
    ↓
Application Layer
    ↓
IAircraftDataService (Interface)
    ↓
InMemoryAircraftDataService (Implementation)
    ↓
ConcurrentDictionary<string, AircraftData>
```

## Future Architecture Considerations

### Planned Enhancements

1. **Data Persistence Layer**
   - Add database support (Entity Framework Core)
   - Implement repository pattern for data access
   - Support multiple storage backends

2. **Real ADS-B Integration**
   - Create `IAdsbReceiver` interface
   - Implement receivers for various ADS-B sources
   - Add real-time data streaming

3. **Additional UI Options**
   - Mobile app (Xamarin/MAUI)
   - Desktop app (WPF/Avalonia)
   - REST API for third-party integrations

4. **Caching Layer**
   - Add in-memory caching for frequently accessed data
   - Implement cache invalidation strategies

5. **Logging & Monitoring**
   - Structured logging with Serilog
   - Application Insights integration
   - Performance monitoring

## Technology Choices

### Why Spectre.Console?

- Rich, modern console UI capabilities
- Cross-platform support
- Excellent documentation
- Active development
- Professional-looking output

### Why Blazor Server?

- Real-time updates with SignalR
- Share code between client and server
- No JavaScript required for logic
- Excellent debugging experience
- Progressive web app capabilities

### Why In-Memory Storage?

For the initial implementation:
- Simple to implement
- No external dependencies
- Fast for demonstration purposes
- Easy to swap out later

### Why .NET 9?

- Latest LTS release (as of project creation)
- Performance improvements
- Modern C# features
- Long-term support from Microsoft

## Security Considerations

### Current Implementation

- In-memory storage (no persistence security needed yet)
- No authentication/authorization (to be added)
- No sensitive data handling

### Future Security Enhancements

1. **Authentication & Authorization**
   - Add user authentication
   - Implement role-based access control
   - API key management for external access

2. **Data Protection**
   - Encrypt sensitive aircraft data
   - Secure communication channels
   - Audit logging

3. **Input Validation**
   - Validate all user inputs
   - Sanitize data before storage
   - Prevent injection attacks

## Performance Considerations

### Current Performance Characteristics

- **Memory**: O(n) where n is number of tracked aircraft
- **Lookup**: O(1) using `ConcurrentDictionary`
- **Concurrent Access**: Thread-safe with `ConcurrentDictionary`

### Scalability

Current implementation supports:
- Hundreds to thousands of aircraft
- Multiple concurrent users (Blazor)
- Real-time updates

For larger scale:
- Add database persistence
- Implement data pagination
- Add caching layers
- Consider distributed architecture

## Testability

The architecture prioritizes testability:

1. **Interface-based design**: Easy to mock dependencies
2. **Dependency injection**: Constructor injection for all dependencies
3. **Pure functions**: Domain logic separated from side effects
4. **No static dependencies**: Everything is injectable
5. **Async throughout**: All I/O operations are async

## Maintainability

Design principles for long-term maintenance:

1. **SOLID Principles**
   - Single Responsibility
   - Open/Closed
   - Liskov Substitution
   - Interface Segregation
   - Dependency Inversion

2. **Clear Naming Conventions**
   - Descriptive names for classes and methods
   - Consistent naming across the solution
   - XML documentation for public APIs

3. **Project Structure**
   - Logical folder organization
   - Clear separation of concerns
   - Minimal coupling between layers

## Documentation

Documentation strategy:
- XML comments for public APIs
- Markdown files for architecture and setup
- README for quick start
- Code examples in documentation
- Architecture diagrams (future)

## Conclusion

This architecture provides a solid foundation for building a robust, testable, and maintainable application for handling ADS-B data. The clean separation of concerns allows for easy extension and modification as requirements evolve.
