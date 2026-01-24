# PlaneCrazy API Integration Documentation

## Overview

PlaneCrazy integrates with the adsb.fi API to fetch real-time aircraft ADS-B (Automatic Dependent Surveillance-Broadcast) data. This document describes the API integration architecture, endpoints used, data mapping, and best practices.

## External API: adsb.fi

### About adsb.fi

**Website**: https://adsb.fi/  
**Type**: Free, open ADS-B data aggregator  
**Coverage**: Global aircraft tracking  
**Update Frequency**: Real-time (typically 1-5 second updates)  
**Data Source**: Crowd-sourced ADS-B receivers worldwide

### Features Used

- Geographic bounding box queries
- Real-time position data
- Aircraft identity information
- No authentication required (public API)
- JSON response format

## API Architecture

### Integration Layer

```
PlaneCrazy Application
    ↓
IAircraftDataService (Domain Interface)
    ↓
AdsbFiAircraftService (Infrastructure Implementation)
    ↓
IApiClient (HTTP Client Abstraction)
    ↓
ApiClient (HttpClient Wrapper)
    ↓
adsb.fi API
```

### Service Abstraction

**Domain Interface** (`IAircraftDataService`):
```csharp
public interface IAircraftDataService
{
    Task<IEnumerable<Aircraft>> FetchAircraftAsync();
}
```

**Benefits**:
- Decouples domain from specific API implementation
- Easy to swap API providers
- Simplifies testing with mock implementations

## API Endpoints

### Bounding Box Query

**Endpoint**: `GET /lat/{lat1}/lon/{lon1}/lat/{lat2}/lon/{lon2}`

**Example**:
```
https://api.adsb.fi/v2/lat/35.0/lon/-10.0/lat/70.0/lon/40.0
```

**Parameters**:
- `lat1`: Minimum latitude (South)
- `lon1`: Minimum longitude (West)
- `lat2`: Maximum latitude (North)
- `lon2`: Maximum longitude (East)

**Current Configuration** (covers most of Europe):
```csharp
var lat1 = 35.0;  // Southern boundary
var lon1 = -10.0; // Western boundary
var lat2 = 70.0;  // Northern boundary
var lon2 = 40.0;  // Eastern boundary
```

### Response Format

**Content-Type**: `application/json`

**Structure**:
```json
{
  "now": 1706102400.5,
  "messages": 125847,
  "total": 156,
  "aircraft": [
    {
      "hex": "abc123",
      "flight": "BAW123  ",
      "alt_baro": 35000,
      "alt_geom": 35525,
      "gs": 450.5,
      "track": 245.2,
      "baro_rate": 0,
      "category": "A3",
      "lat": 51.5074,
      "lon": -0.1278,
      "seen": 0.5,
      "rssi": -23.4,
      "messages": 542,
      "seen_pos": 1.2,
      "r": "G-ABCD",
      "t": "B738"
    }
  ]
}
```

### Response Fields

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `hex` | string | ICAO24 identifier (unique) | "abc123" |
| `flight` | string | Callsign (trimmed) | "BAW123" |
| `r` | string | Registration | "G-ABCD" |
| `t` | string | Aircraft type | "B738" |
| `lat` | number | Latitude | 51.5074 |
| `lon` | number | Longitude | -0.1278 |
| `alt_baro` | number | Barometric altitude (feet) | 35000 |
| `alt_geom` | number | Geometric altitude (feet) | 35525 |
| `gs` | number | Ground speed (knots) | 450.5 |
| `track` | number | Track/heading (degrees) | 245.2 |
| `baro_rate` | number | Vertical rate (ft/min) | 0 |
| `geom_rate` | number | Geometric vertical rate | 64 |
| `squawk` | string | Transponder code | "7700" |
| `emergency` | string | Emergency status | "none" |
| `category` | string | Aircraft category | "A3" |
| `nav_altitude_mcp` | number | Autopilot altitude | 36000 |
| `seen` | number | Seconds since last message | 0.5 |
| `seen_pos` | number | Seconds since position update | 1.2 |
| `rssi` | number | Signal strength | -23.4 |
| `messages` | number | Message count | 542 |

## Implementation

### AdsbFiAircraftService

**Location**: `PlaneCrazy.Infrastructure/Services/AdsbFiAircraftService.cs`

```csharp
public class AdsbFiAircraftService : IAircraftDataService
{
    private const string BaseUrl = "https://api.adsb.fi/v2";
    private readonly IApiClient _apiClient;
    
    public AdsbFiAircraftService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    public async Task<IEnumerable<Aircraft>> FetchAircraftAsync()
    {
        try
        {
            // Bounding box for Europe
            var lat1 = 35.0;
            var lon1 = -10.0;
            var lat2 = 70.0;
            var lon2 = 40.0;

            var url = $"{BaseUrl}/lat/{lat1}/lon/{lon1}/lat/{lat2}/lon/{lon2}";
            var response = await _apiClient.GetAsync<AdsbFiSnapshot>(url);

            if (response?.Aircraft == null)
            {
                return Enumerable.Empty<Aircraft>();
            }

            return response.Aircraft
                .Select(MapToAircraft)
                .Where(a => a != null)
                .Cast<Aircraft>();
        }
        catch
        {
            return Enumerable.Empty<Aircraft>();
        }
    }
}
```

### Data Mapping

**External Model** (`AdsbFiAircraft`):
```csharp
public class AdsbFiAircraft
{
    [JsonPropertyName("hex")]
    public string? Hex { get; set; }
    
    [JsonPropertyName("flight")]
    public string? Flight { get; set; }
    
    [JsonPropertyName("r")]
    public string? R { get; set; }
    
    [JsonPropertyName("t")]
    public string? T { get; set; }
    
    [JsonPropertyName("lat")]
    public double? Lat { get; set; }
    
    [JsonPropertyName("lon")]
    public double? Lon { get; set; }
    
    // ... other fields
}
```

**Domain Model** (`Aircraft`):
```csharp
public class Aircraft
{
    public required string Icao24 { get; set; }
    public string? Registration { get; set; }
    public string? TypeCode { get; set; }
    public string? Callsign { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    // ... other fields
}
```

**Mapping Function**:
```csharp
private Aircraft? MapToAircraft(AdsbFiAircraft adsbAircraft)
{
    if (string.IsNullOrWhiteSpace(adsbAircraft.Hex))
        return null;

    return new Aircraft
    {
        Icao24 = adsbAircraft.Hex.ToUpper(),
        Registration = adsbAircraft.R?.Trim(),
        TypeCode = adsbAircraft.T?.Trim(),
        Callsign = adsbAircraft.Flight?.Trim(),
        Latitude = adsbAircraft.Lat,
        Longitude = adsbAircraft.Lon,
        Altitude = adsbAircraft.Alt_Baro,
        Velocity = adsbAircraft.Gs,
        Track = adsbAircraft.Track,
        VerticalRate = adsbAircraft.Baro_Rate,
        OnGround = adsbAircraft.Alt_Baro == 0,
        Squawk = adsbAircraft.Squawk,
        FirstSeen = DateTime.UtcNow,
        LastSeen = DateTime.UtcNow
    };
}
```

## HTTP Client Configuration

### ApiClient Implementation

**Location**: `PlaneCrazy.Infrastructure/Http/ApiClient.cs`

```csharp
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    
    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<T?> GetAsync<T>(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));

        return await _httpClient.GetFromJsonAsync<T>(url);
    }
}
```

### HttpClient Factory

**Registration** in `ServiceCollectionExtensions.cs`:
```csharp
services.AddHttpClient<IApiClient, ApiClient>();
```

**Benefits**:
- Automatic HttpClient lifecycle management
- Connection pooling
- DNS refresh handling
- Request/response pipeline extensibility

## Error Handling

### Network Errors

```csharp
public async Task<IEnumerable<Aircraft>> FetchAircraftAsync()
{
    try
    {
        var response = await _apiClient.GetAsync<AdsbFiSnapshot>(url);
        
        if (response?.Aircraft == null)
        {
            return Enumerable.Empty<Aircraft>();
        }
        
        return response.Aircraft.Select(MapToAircraft);
    }
    catch (HttpRequestException ex)
    {
        _logger?.LogError(ex, "HTTP error fetching aircraft data");
        return Enumerable.Empty<Aircraft>();
    }
    catch (TaskCanceledException ex)
    {
        _logger?.LogError(ex, "Request timeout fetching aircraft data");
        return Enumerable.Empty<Aircraft>();
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "Unexpected error fetching aircraft data");
        return Enumerable.Empty<Aircraft>();
    }
}
```

### Graceful Degradation

- **API unavailable**: Return empty collection
- **Network timeout**: Log error, continue polling
- **Invalid response**: Skip invalid aircraft, process valid ones
- **No data**: Return empty collection (not an error)

## Rate Limiting

### Current Approach

**No explicit rate limiting** - adsb.fi API is public and free

**Polling Interval**: 30 seconds (configurable)

**Why This Works**:
- API designed for frequent polling
- Reasonable request frequency
- No rate limit headers observed

### Future Considerations

If rate limiting becomes necessary:

```csharp
public class RateLimitedApiClient : IApiClient
{
    private readonly SemaphoreSlim _rateLimiter = new(1, 1);
    private DateTime _lastRequest = DateTime.MinValue;
    private readonly TimeSpan _minimumInterval = TimeSpan.FromSeconds(1);
    
    public async Task<T?> GetAsync<T>(string url)
    {
        await _rateLimiter.WaitAsync();
        try
        {
            var elapsed = DateTime.UtcNow - _lastRequest;
            if (elapsed < _minimumInterval)
            {
                await Task.Delay(_minimumInterval - elapsed);
            }
            
            _lastRequest = DateTime.UtcNow;
            return await _httpClient.GetFromJsonAsync<T>(url);
        }
        finally
        {
            _rateLimiter.Release();
        }
    }
}
```

## Caching Strategy

### No Explicit Caching

**Rationale**:
- Background poller provides continuous updates
- Repository serves as implicit cache
- Real-time data is the primary use case

### Repository as Cache

```
API → AdsbFiAircraftService → AircraftRepository (in-memory)
```

**Benefits**:
- Last known aircraft state always available
- Survives temporary API outages
- Fast local queries

## Testing

### Mocking the API

```csharp
public class MockAircraftDataService : IAircraftDataService
{
    public Task<IEnumerable<Aircraft>> FetchAircraftAsync()
    {
        return Task.FromResult<IEnumerable<Aircraft>>(new[]
        {
            new Aircraft
            {
                Icao24 = "ABC123",
                Registration = "N12345",
                TypeCode = "B738",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Altitude = 35000
            }
        });
    }
}
```

### Integration Test Example

```csharp
[Test]
public async Task FetchAircraftAsync_RealApi_ReturnsData()
{
    // Arrange
    var httpClient = new HttpClient();
    var apiClient = new ApiClient(httpClient);
    var service = new AdsbFiAircraftService(apiClient);
    
    // Act
    var aircraft = await service.FetchAircraftAsync();
    
    // Assert
    Assert.That(aircraft, Is.Not.Null);
    // Note: May return empty if no aircraft in area
}
```

## Alternative API Providers

### OpenSky Network

**URL**: https://opensky-network.org/  
**API**: https://openskynetwork.github.io/opensky-api/

**Pros**:
- Well-documented API
- Academic backing
- Historical data available

**Cons**:
- Rate limiting (more restrictive)
- Authentication required for higher limits

**Migration Path**:
1. Implement `IOpenSkyAircraftService : IAircraftDataService`
2. Map OpenSky response format to domain model
3. Register in DI container
4. No changes needed to rest of application

### FlightRadar24 (Commercial)

**URL**: https://www.flightradar24.com/  

**Pros**:
- Comprehensive coverage
- Rich metadata
- High reliability

**Cons**:
- Requires paid API access
- Commercial license needed

### ADS-B Exchange

**URL**: https://www.adsbexchange.com/  
**API**: https://www.adsbexchange.com/data/

**Pros**:
- Unfiltered data (includes military)
- High update frequency
- Community-driven

**Cons**:
- Requires API key
- Rate limits based on tier

## Configuration

### Bounding Box Configuration

**Current** (hardcoded in service):
```csharp
var lat1 = 35.0;  // Southern Europe
var lon1 = -10.0;
var lat2 = 70.0;  // Northern Europe
var lon2 = 40.0;
```

**Future** (configurable):
```csharp
public class AdsbFiConfiguration
{
    public double MinLatitude { get; set; } = 35.0;
    public double MinLongitude { get; set; } = -10.0;
    public double MaxLatitude { get; set; } = 70.0;
    public double MaxLongitude { get; set; } = 40.0;
    public string BaseUrl { get; set; } = "https://api.adsb.fi/v2";
}
```

### Common Bounding Boxes

**Europe**:
```csharp
lat1: 35.0, lon1: -10.0, lat2: 70.0, lon2: 40.0
```

**North America**:
```csharp
lat1: 25.0, lon1: -125.0, lat2: 50.0, lon2: -65.0
```

**United Kingdom**:
```csharp
lat1: 49.5, lon1: -8.0, lat2: 61.0, lon2: 2.0
```

**Global** (not recommended - too much data):
```csharp
lat1: -90.0, lon1: -180.0, lat2: 90.0, lon2: 180.0
```

## Performance Optimization

### Response Size

**Typical Response**: 20-200 aircraft  
**JSON Size**: 10-100 KB  
**Transfer Time**: < 1 second

### Parallel Queries

For multiple regions:
```csharp
public async Task<IEnumerable<Aircraft>> FetchMultipleRegionsAsync()
{
    var tasks = new[]
    {
        FetchRegionAsync(35.0, -10.0, 50.0, 10.0), // Region 1
        FetchRegionAsync(50.0, -10.0, 70.0, 10.0)  // Region 2
    };
    
    var results = await Task.WhenAll(tasks);
    return results.SelectMany(r => r);
}
```

### Compression

Enable compression for reduced bandwidth:
```csharp
services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.DefaultRequestHeaders.AcceptEncoding.Add(
        new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
});
```

## Monitoring

### API Health Checks

```csharp
public class AdsbFiHealthCheck : IHealthCheck
{
    private readonly IAircraftDataService _service;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var aircraft = await _service.FetchAircraftAsync();
            return HealthCheckResult.Healthy($"Fetched {aircraft.Count()} aircraft");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("API unavailable", ex);
        }
    }
}
```

### Metrics

Track API performance:
```csharp
private async Task<T?> GetWithMetricsAsync<T>(string url)
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        var result = await _httpClient.GetFromJsonAsync<T>(url);
        stopwatch.Stop();
        
        _logger?.LogInformation(
            "API call to {Url} completed in {Ms}ms",
            url, stopwatch.ElapsedMilliseconds);
        
        return result;
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        _logger?.LogError(ex, 
            "API call to {Url} failed after {Ms}ms",
            url, stopwatch.ElapsedMilliseconds);
        throw;
    }
}
```

## Best Practices

1. **Always handle API failures gracefully**
2. **Return empty collections rather than null**
3. **Log all API errors for monitoring**
4. **Use HttpClientFactory for connection pooling**
5. **Map to domain models immediately**
6. **Validate external data before using**
7. **Don't expose external models to domain layer**
8. **Cache responses when appropriate**
9. **Implement retry logic for transient failures**
10. **Monitor API response times and error rates**

## Future Enhancements

1. **Multiple API Support**: Load balancing across providers
2. **Fallback Providers**: Automatic failover to backup API
3. **Enhanced Caching**: Redis or memory cache for API responses
4. **Historical Data**: Query historical flight paths
5. **Weather Data**: Integrate weather API for conditions
6. **Airport Data**: Fetch airport information from dedicated API
7. **Aircraft Database**: Rich metadata from aviation databases
8. **Route Prediction**: Machine learning for flight path prediction
9. **Real-time Notifications**: WebSocket support for live updates
10. **Data Validation**: Enhanced validation of API response data

## Troubleshooting

### No Aircraft Returned

**Possible Causes**:
1. No aircraft in bounding box
2. API temporarily unavailable
3. Network connectivity issues
4. Invalid bounding box coordinates

**Solutions**:
- Expand bounding box
- Check API status
- Verify internet connection
- Review log files

### Stale Data

**Possible Causes**:
1. Background poller disabled
2. API rate limited
3. Network issues

**Solutions**:
- Check `PollerConfiguration.Enabled`
- Review polling interval
- Check logs for errors

### Invalid Aircraft Data

**Possible Causes**:
1. Malformed API response
2. Data mapping errors
3. Missing required fields

**Solutions**:
- Add validation in mapping function
- Log invalid records for analysis
- Skip invalid aircraft gracefully

---

*Last Updated: January 24, 2026*
