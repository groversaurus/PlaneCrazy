# Dependency Injection Setup

This document describes the dependency injection configuration for the PlaneCrazy application.

## Overview

PlaneCrazy uses Microsoft.Extensions.DependencyInjection to provide a robust, modular, and testable dependency injection setup. All infrastructure services, repositories, event stores, projections, and HTTP clients are registered through the DI container.

## Architecture

The DI setup follows a layered architecture:

- **Domain Layer**: Contains interfaces and domain models (no dependencies)
- **Infrastructure Layer**: Contains implementations and provides extension methods for service registration
- **Console Layer**: Entry point that configures and uses the DI container

## Service Registration

All services are registered in the `ServiceCollectionExtensions` class located in:
```
src/PlaneCrazy.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs
```

### Extension Method

The main extension method is `AddPlaneCrazyInfrastructure()`, which registers all required services:

```csharp
services.AddPlaneCrazyInfrastructure();
```

## Registered Services

### Event Store
- **Interface**: `IEventStore`
- **Implementation**: `JsonFileEventStore`
- **Lifetime**: Singleton
- **Purpose**: Maintains event history state across the application lifetime

### Repositories
- **Implementations**: 
  - `AircraftRepository`
  - `FavouriteRepository`
  - `CommentRepository`
- **Lifetime**: Singleton
- **Purpose**: File-based repositories with thread-safe operations for data persistence

### Projections
- **Implementations**:
  - `FavouriteProjection`
  - `CommentProjection`
- **Lifetime**: Singleton
- **Purpose**: Stateless projection logic to build read models from events

### HTTP Services
- **Interface**: `IApiClient`
- **Implementation**: `ApiClient`
- **Lifetime**: Managed by HttpClientFactory
- **Purpose**: REST API client for making HTTP requests

### Aircraft Data Service
- **Interface**: `IAircraftDataService`
- **Implementation**: `AdsbFiAircraftService`
- **Lifetime**: Singleton
- **Purpose**: Fetches aircraft data from external ADS-B API

## Service Lifetimes

### Singleton
All services are registered as Singleton because:
1. The console application is long-running
2. Repositories use thread-safe file operations with semaphores
3. Services are stateless or maintain state intentionally (event store)
4. No per-request or per-operation state is required

### HttpClient
The `IApiClient` implementation uses `AddHttpClient()`, which automatically manages:
- HttpClient lifecycle
- Connection pooling
- DNS refresh
- Handler pipeline

## Usage in Console Application

### Configuration

In `Program.cs`, services are configured during startup:

```csharp
private static void ConfigureServices()
{
    var services = new ServiceCollection();
    
    // Add all infrastructure services
    services.AddPlaneCrazyInfrastructure();

    _serviceProvider = services.BuildServiceProvider();
}
```

### Service Resolution

Services are resolved from the container:

```csharp
private static void InitializeApp()
{
    _eventStore = _serviceProvider.GetRequiredService<IEventStore>();
    _aircraftRepo = _serviceProvider.GetRequiredService<AircraftRepository>();
    _favouriteRepo = _serviceProvider.GetRequiredService<FavouriteRepository>();
    _commentRepo = _serviceProvider.GetRequiredService<CommentRepository>();
    _aircraftService = _serviceProvider.GetRequiredService<IAircraftDataService>();
    _favouriteProjection = _serviceProvider.GetRequiredService<FavouriteProjection>();
    _commentProjection = _serviceProvider.GetRequiredService<CommentProjection>();
}
```

## Testing Benefits

The DI setup provides several testing advantages:

1. **Interface-Based Design**: All services depend on interfaces, allowing easy mocking
2. **Constructor Injection**: Dependencies are explicit and testable
3. **Service Replacement**: Mock services can be registered in place of real ones
4. **Isolation**: Each service can be tested independently

### Example Test Setup

```csharp
// Example: Testing with mock services
var services = new ServiceCollection();

// Register mock services
services.AddSingleton<IEventStore>(mockEventStore);
services.AddSingleton<AircraftRepository>(mockAircraftRepo);

// Register real services under test
services.AddSingleton<FavouriteProjection>();

var serviceProvider = services.BuildServiceProvider();
var projection = serviceProvider.GetRequiredService<FavouriteProjection>();
```

## Extensibility

To add new services:

1. Define the interface in `PlaneCrazy.Domain.Interfaces`
2. Implement the interface in `PlaneCrazy.Infrastructure`
3. Register the service in `ServiceCollectionExtensions.AddPlaneCrazyInfrastructure()`
4. Resolve the service in `Program.cs` or inject it into other services

### Example: Adding a New Service

```csharp
// 1. Define interface
public interface IWeatherService
{
    Task<Weather> GetWeatherAsync(double lat, double lon);
}

// 2. Implement
public class WeatherService : IWeatherService
{
    private readonly IApiClient _apiClient;
    
    public WeatherService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    public async Task<Weather> GetWeatherAsync(double lat, double lon)
    {
        // Implementation
    }
}

// 3. Register in ServiceCollectionExtensions
services.AddSingleton<IWeatherService, WeatherService>();

// 4. Resolve in Program.cs
_weatherService = _serviceProvider.GetRequiredService<IWeatherService>();
```

## Dependencies

### NuGet Packages

**Console Project**:
- `Microsoft.Extensions.DependencyInjection` (10.0.2)
- `Microsoft.Extensions.DependencyInjection.Abstractions` (10.0.2)

**Infrastructure Project**:
- `Microsoft.Extensions.DependencyInjection.Abstractions` (10.0.2)
- `Microsoft.Extensions.Http` (10.0.2)

## Best Practices

1. **Always use interfaces**: Define contracts in the Domain layer
2. **Constructor injection**: Prefer constructor injection over property injection
3. **Single responsibility**: Each service should have a single, well-defined purpose
4. **Appropriate lifetimes**: Choose Singleton, Scoped, or Transient based on service requirements
5. **Avoid service locator**: Resolve services at the composition root, not throughout the application
6. **Document dependencies**: Clearly document service dependencies and lifetimes

## Future Enhancements

Potential improvements to the DI setup:

1. **Configuration**: Add `IConfiguration` support for service configuration
2. **Logging**: Integrate `ILogger` for structured logging
3. **Health checks**: Add health check services for monitoring
4. **Options pattern**: Use `IOptions<T>` for strongly-typed configuration
5. **Hosted services**: Implement `IHostedService` for background tasks
6. **Scoped lifetimes**: Consider scoped services if per-operation state becomes necessary

## Troubleshooting

### Common Issues

**Service not found**:
- Ensure the service is registered in `ServiceCollectionExtensions`
- Check that you're using the correct interface type

**Circular dependencies**:
- Review service dependencies
- Consider using `Lazy<T>` or breaking the cycle with an abstraction

**Disposed services**:
- Verify service lifetimes match usage patterns
- Avoid storing scoped services in singleton services

## References

- [Microsoft Dependency Injection Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Service Lifetimes](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [HttpClient Factory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
