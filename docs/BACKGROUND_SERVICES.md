# Background Services

PlaneCrazy includes background services that run continuously while the application is active, enabling automated aircraft tracking and data processing.

## BackgroundAdsBPoller

The `BackgroundAdsBPoller` is a hosted service that periodically polls adsb.fi for aircraft data and processes changes.

### Features

- **Automatic Polling**: Fetches aircraft data at configurable intervals (default: 30 seconds)
- **Event Detection**: Automatically detects and emits domain events for aircraft changes
- **Non-Blocking**: Runs in the background without interfering with the interactive console menu
- **Graceful Shutdown**: Properly stops when the application exits
- **Error Handling**: Continues polling even if individual fetch operations fail
- **Comprehensive Logging**: Logs all polling activity and errors

### Configuration

The poller can be configured through the `PollerConfiguration` class:

```csharp
public class PollerConfiguration
{
    // Enable/disable the poller
    public bool Enabled { get; set; } = true;

    // Polling interval in seconds
    public int PollingIntervalSeconds { get; set; } = 30;

    // Bounding box for adsb.fi queries (lat/lon)
    public double MinLatitude { get; set; } = 35.0;
    public double MinLongitude { get; set; } = -10.0;
    public double MaxLatitude { get; set; } = 70.0;
    public double MaxLongitude { get; set; } = 40.0;

    // Timeout for missing aircraft (minutes)
    public int MissingAircraftTimeoutMinutes { get; set; } = 5;
}
```

To customize the configuration, modify the registration in `ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<PollerConfiguration>(new PollerConfiguration
{
    PollingIntervalSeconds = 60, // Poll every minute
    Enabled = true
});
```

### Event Detection Logic

The poller implements the following logic for each polling cycle:

#### New Aircraft
When an aircraft is detected for the first time:
1. Emits `AircraftFirstSeen` event with initial position
2. Emits `AircraftIdentityUpdated` event if identity data is available
3. Emits `AircraftPositionUpdated` event if position data is available
4. Adds aircraft to the repository

#### Existing Aircraft
When an aircraft already exists in the repository:
1. **Position Changed**: Emits `AircraftPositionUpdated` if any position-related field changes
   - Latitude, Longitude, Altitude
   - Velocity, Track, VerticalRate
   - OnGround status
2. **Identity Changed**: Emits `AircraftIdentityUpdated` if identity fields change
   - Callsign, Registration, TypeCode
3. Updates `LastSeen` timestamp in the repository

#### Missing Aircraft
When an aircraft is in the repository but not in the latest fetch:
1. Checks if it hasn't been seen for more than 5 minutes (configurable)
2. If timeout exceeded, emits `AircraftLastSeen` event with last known position

### Event Flow

```
┌─────────────────────┐
│ BackgroundAdsBPoller│
└──────────┬──────────┘
           │
           ▼
   ┌───────────────┐
   │IAircraftData  │
   │Service        │
   └───────┬───────┘
           │
           ▼
   ┌───────────────┐
   │Event Detection│
   └───────┬───────┘
           │
           ▼
   ┌───────────────┐
   │IEventDispatcher│
   └───────┬───────┘
           │
           ▼
   ┌───────────────────┐
   │ IEventStore      │
   │ + Projections    │
   └──────────────────┘
```

### Logging

The poller logs the following information:

- **Info**: Service start/stop, aircraft counts, processing summaries
- **Debug**: Individual aircraft processing, fetch operations
- **Error**: API failures, exceptions during processing
- **Warning**: No data fetched, empty responses

Example log output:
```
[INFO] Starting Background ADS-B poller with interval: 30 seconds
[INFO] Fetched 47 aircraft from adsb.fi
[DEBUG] New aircraft detected: 4CA123
[DEBUG] Aircraft 3C6789 not seen for 5 minutes, emitting LastSeen event
[INFO] Processing complete: 3 new, 42 updated, 2 missing aircraft
```

### Thread Safety

The poller is designed to be thread-safe:
- Uses `_isPolling` flag to prevent concurrent polling cycles
- All repository operations use `SemaphoreSlim` for synchronization
- Event dispatching is sequential within each cycle

### Error Handling

The poller implements robust error handling:
- API failures are caught and logged, but don't stop the polling loop
- Individual aircraft processing errors are isolated
- The service continues running even after repeated failures

### Performance Considerations

- Each polling cycle is independent and doesn't block subsequent cycles
- The poller skips a cycle if the previous one is still running
- Repository operations are optimized with in-memory caching
- Event dispatching is asynchronous but sequential to maintain order

## Running with Background Services

The application uses `Microsoft.Extensions.Hosting` to enable background services:

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddPlaneCrazyInfrastructure();

var host = builder.Build();
await host.RunAsync(); // Start background services
```

The console menu remains interactive while background services run. When the user exits, the application gracefully stops all background services before terminating.

## Disabling the Poller

To disable the background poller without removing it:

```csharp
services.AddSingleton<PollerConfiguration>(new PollerConfiguration
{
    Enabled = false
});
```

Or remove the hosted service registration:

```csharp
// Comment out this line in ServiceCollectionExtensions.cs
// services.AddHostedService<BackgroundAdsBPoller>();
```

## Extending Background Services

To add additional background services:

1. Create a class implementing `IHostedService`
2. Register it in `ServiceCollectionExtensions.cs`:
   ```csharp
   services.AddHostedService<YourBackgroundService>();
   ```
3. The service will automatically start with the application

## Troubleshooting

### Poller not starting
- Check that `PollerConfiguration.Enabled` is `true`
- Verify the hosted service is registered in DI
- Check logs for startup errors

### No events being emitted
- Verify adsb.fi API is accessible
- Check the bounding box configuration
- Review error logs for API failures

### Performance issues
- Increase `PollingIntervalSeconds` to reduce API call frequency
- Check repository file sizes (large files slow down I/O)
- Monitor log levels (reduce to `Warning` in production)
