# PlaneCrazy Performance Guide

## Overview

This document covers performance considerations, optimization strategies, and best practices for ensuring PlaneCrazy runs efficiently.

## Performance Characteristics

### Current Performance Profile

**Startup Time**: < 5 seconds  
**Memory Usage**: 50-200 MB (steady state)  
**CPU Usage**: < 5% (between polling cycles)  
**Disk I/O**: Moderate (event store writes)  
**Network**: Minimal (periodic API calls)

### Performance Goals

- **Responsiveness**: Commands complete in < 100ms
- **Throughput**: Handle 1000+ aircraft per poll
- **Scalability**: Support millions of events
- **Reliability**: 99.9% uptime

---

## CPU Performance

### Optimization Strategies

#### 1. Reduce Polling Frequency

**Impact**: Significant CPU reduction

```csharp
// Low frequency (production)
public class PollerConfiguration
{
    public int IntervalSeconds { get; set; } = 60; // Every minute
}

// High frequency (development)
public class PollerConfiguration
{
    public int IntervalSeconds { get; set; } = 10; // Every 10 seconds
}
```

**Trade-offs**:
- Lower frequency = Less CPU, less fresh data
- Higher frequency = More CPU, fresher data

#### 2. Parallel Processing

**Example**: Process aircraft data in parallel

```csharp
public async Task ProcessAircraftAsync(IEnumerable<Aircraft> aircraft)
{
    var tasks = aircraft.Select(async a => 
    {
        await ProcessSingleAircraftAsync(a);
    });
    
    await Task.WhenAll(tasks);
}
```

**Caution**: Don't over-parallelize (context switching overhead)

#### 3. Efficient Algorithms

**Example**: Use hash sets for lookups instead of lists

```csharp
// ❌ Slow: O(n) lookup
private List<string> _favourites = new();
public bool IsFavourited(string icao24) => _favourites.Contains(icao24);

// ✅ Fast: O(1) lookup
private HashSet<string> _favourites = new();
public bool IsFavourited(string icao24) => _favourites.Contains(icao24);
```

#### 4. Avoid Unnecessary Work

**Example**: Skip processing if data hasn't changed

```csharp
public void UpdateAircraft(Aircraft newData)
{
    if (_currentData.Equals(newData))
    {
        return; // No change, skip processing
    }
    
    // Process update
    ProcessAircraftUpdate(newData);
}
```

---

## Memory Performance

### Memory Management

#### 1. Event Store Growth

**Problem**: Unlimited event growth leads to memory issues

**Solution 1**: Archive old events
```csharp
public class EventArchiver
{
    public async Task ArchiveOldEventsAsync(DateTime before)
    {
        var oldEvents = await _eventStore.GetEventsBeforeAsync(before);
        
        // Compress and move to archive
        var archivePath = Path.Combine(_archiveDirectory, 
            $"events_{before:yyyyMMdd}.json.gz");
        
        await CompressAndSaveAsync(oldEvents, archivePath);
        
        // Remove from active store
        await _eventStore.DeleteAsync(oldEvents);
    }
}
```

**Solution 2**: Implement snapshots
```csharp
public class SnapshotService
{
    public async Task CreateSnapshotAsync()
    {
        // Capture current state
        var snapshot = new Snapshot
        {
            Timestamp = DateTime.UtcNow,
            Favourites = await _favouriteRepo.GetAllAsync(),
            Comments = await _commentRepo.GetAllAsync(),
            AircraftStates = await _aircraftRepo.GetAllAsync()
        };
        
        // Save snapshot
        await SaveSnapshotAsync(snapshot);
        
        // Archive events before snapshot
        await ArchiveEventsBeforeAsync(snapshot.Timestamp);
    }
}
```

#### 2. Projection Optimization

**Problem**: Large in-memory projections

**Solution**: Implement paging
```csharp
public class PagedAircraftProjection
{
    private const int PageSize = 1000;
    
    public async Task<IEnumerable<Aircraft>> GetPageAsync(int page)
    {
        var skip = page * PageSize;
        var events = await _eventStore.GetEventsAsync(skip, PageSize);
        
        return BuildProjectionFromEvents(events);
    }
}
```

#### 3. String Interning

**For frequently repeated strings**:

```csharp
public class AircraftFactory
{
    public Aircraft CreateAircraft(AdsbFiAircraft source)
    {
        return new Aircraft
        {
            // Intern type code (many aircraft have same type)
            TypeCode = string.IsInterned(source.T) ?? string.Intern(source.T),
            
            // Don't intern unique identifiers
            Icao24 = source.Hex.ToUpper()
        };
    }
}
```

#### 4. Object Pooling

**For frequently allocated objects**:

```csharp
public class AircraftPool
{
    private readonly ObjectPool<Aircraft> _pool = 
        new DefaultObjectPool<Aircraft>(new AircraftPoolPolicy());
    
    public Aircraft Rent()
    {
        return _pool.Get();
    }
    
    public void Return(Aircraft aircraft)
    {
        _pool.Return(aircraft);
    }
}

public class AircraftPoolPolicy : IPooledObjectPolicy<Aircraft>
{
    public Aircraft Create() => new Aircraft();
    
    public bool Return(Aircraft obj)
    {
        // Reset state
        obj.Reset();
        return true;
    }
}
```

---

## Disk I/O Performance

### Optimization Strategies

#### 1. Batch Writes

**Problem**: Individual file writes are slow

**Solution**: Batch multiple events
```csharp
public class BatchedEventStore : IEventStore
{
    private readonly List<DomainEvent> _pendingEvents = new();
    private readonly int _batchSize = 10;
    
    public async Task AppendAsync(DomainEvent @event)
    {
        _pendingEvents.Add(@event);
        
        if (_pendingEvents.Count >= _batchSize)
        {
            await FlushAsync();
        }
    }
    
    public async Task FlushAsync()
    {
        if (_pendingEvents.Any())
        {
            await WriteBatchAsync(_pendingEvents);
            _pendingEvents.Clear();
        }
    }
}
```

#### 2. Asynchronous I/O

**Always use async file operations**:

```csharp
// ❌ Synchronous (blocks thread)
File.WriteAllText(path, json);

// ✅ Asynchronous (non-blocking)
await File.WriteAllTextAsync(path, json);
```

#### 3. Buffered Streams

**For large files**:

```csharp
public async Task WriteEventsAsync(IEnumerable<DomainEvent> events, string path)
{
    using var fileStream = new FileStream(path, FileMode.Create, 
        FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
    
    using var writer = new StreamWriter(fileStream);
    
    await foreach (var @event in events)
    {
        var json = JsonSerializer.Serialize(@event);
        await writer.WriteLineAsync(json);
    }
}
```

#### 4. Compression

**Reduce disk usage and I/O time**:

```csharp
public async Task WriteCompressedAsync(string path, string data)
{
    using var fileStream = File.Create(path + ".gz");
    using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
    using var writer = new StreamWriter(gzipStream);
    
    await writer.WriteAsync(data);
}
```

#### 5. SSD Optimization

**For SSDs, avoid unnecessary writes**:

```csharp
public class SsdOptimizedStore
{
    // Write only when needed
    private DateTime _lastWrite;
    private readonly TimeSpan _writeThrottle = TimeSpan.FromSeconds(5);
    
    public async Task SaveAsync(string data)
    {
        if (DateTime.UtcNow - _lastWrite < _writeThrottle)
        {
            return; // Skip write (too frequent)
        }
        
        await File.WriteAllTextAsync(_path, data);
        _lastWrite = DateTime.UtcNow;
    }
}
```

---

## Network Performance

### API Optimization

#### 1. Request Caching

**Cache API responses**:

```csharp
public class CachedAircraftService : IAircraftDataService
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);
    
    public async Task<IEnumerable<Aircraft>> FetchAircraftAsync()
    {
        const string cacheKey = "aircraft_data";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<Aircraft>? cached))
        {
            return cached!;
        }
        
        var aircraft = await _innerService.FetchAircraftAsync();
        
        _cache.Set(cacheKey, aircraft, _cacheDuration);
        
        return aircraft;
    }
}
```

#### 2. Connection Pooling

**Use HttpClientFactory**:

```csharp
// Automatically handles connection pooling
services.AddHttpClient<IApiClient, ApiClient>();
```

#### 3. Request Timeout

**Prevent hanging requests**:

```csharp
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(10)
};
```

#### 4. Compression

**Enable response compression**:

```csharp
var handler = new HttpClientHandler
{
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
};

var httpClient = new HttpClient(handler);
```

---

## Query Performance

### Projection Optimization

#### 1. Index Projections

**Add indexes for common queries**:

```csharp
public class IndexedFavouriteProjection : IProjection
{
    private readonly Dictionary<string, Favourite> _byId = new();
    private readonly Dictionary<string, List<Favourite>> _byType = new();
    
    public void Apply(DomainEvent @event)
    {
        if (@event is AircraftFavourited af)
        {
            var fav = new Favourite { Icao24 = af.Icao24 };
            
            // Index by ID
            _byId[af.Icao24] = fav;
            
            // Index by type
            if (!_byType.ContainsKey("Aircraft"))
                _byType["Aircraft"] = new List<Favourite>();
            
            _byType["Aircraft"].Add(fav);
        }
    }
    
    // Fast lookup by ID: O(1)
    public Favourite? GetById(string id) => _byId.GetValueOrDefault(id);
    
    // Fast lookup by type: O(1) + O(n) where n = favourites of that type
    public IEnumerable<Favourite> GetByType(string type) => 
        _byType.GetValueOrDefault(type) ?? Enumerable.Empty<Favourite>();
}
```

#### 2. Materialized Views

**Pre-compute expensive queries**:

```csharp
public class CommentStatisticsView
{
    private int _totalComments;
    private Dictionary<string, int> _commentsByType = new();
    
    public void Update(DomainEvent @event)
    {
        if (@event is CommentAdded ca)
        {
            _totalComments++;
            
            if (!_commentsByType.ContainsKey(ca.EntityType))
                _commentsByType[ca.EntityType] = 0;
            
            _commentsByType[ca.EntityType]++;
        }
        else if (@event is CommentDeleted cd)
        {
            _totalComments--;
            // Update type count
        }
    }
    
    // Instant query (pre-computed)
    public int GetTotalComments() => _totalComments;
    public int GetCommentsByType(string type) => 
        _commentsByType.GetValueOrDefault(type);
}
```

#### 3. Lazy Loading

**Load data only when needed**:

```csharp
public class LazyAircraftDetails
{
    private Aircraft? _aircraft;
    private readonly string _icao24;
    private readonly IAircraftRepository _repo;
    
    public async Task<Aircraft> GetAircraftAsync()
    {
        if (_aircraft == null)
        {
            _aircraft = await _repo.GetByIdAsync(_icao24);
        }
        
        return _aircraft;
    }
}
```

---

## Concurrency Performance

### Thread Safety

#### 1. Lock Contention

**Problem**: Excessive locking slows down system

**Solution**: Use concurrent collections
```csharp
// ❌ Lock contention
private readonly object _lock = new();
private readonly Dictionary<string, Aircraft> _aircraft = new();

public void Add(Aircraft aircraft)
{
    lock (_lock)
    {
        _aircraft[aircraft.Icao24] = aircraft;
    }
}

// ✅ Lock-free concurrent collection
private readonly ConcurrentDictionary<string, Aircraft> _aircraft = new();

public void Add(Aircraft aircraft)
{
    _aircraft[aircraft.Icao24] = aircraft;
}
```

#### 2. Async/Await Performance

**Avoid unnecessary async**:

```csharp
// ❌ Unnecessary async overhead
public async Task<string> GetIcao24Async(Aircraft aircraft)
{
    return await Task.FromResult(aircraft.Icao24); // Wasteful
}

// ✅ Synchronous (no overhead)
public string GetIcao24(Aircraft aircraft)
{
    return aircraft.Icao24;
}
```

#### 3. ConfigureAwait

**In library code**:

```csharp
public async Task<Aircraft> LoadAircraftAsync(string icao24)
{
    // Don't capture context (faster)
    var data = await _repo.GetByIdAsync(icao24).ConfigureAwait(false);
    return data;
}
```

---

## Benchmarking

### Performance Testing

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class EventStoreBenchmarks
{
    private JsonFileEventStore _store;
    private DomainEvent _event;
    
    [GlobalSetup]
    public void Setup()
    {
        _store = new JsonFileEventStore(testPath);
        _event = new CommentAdded 
        { 
            EntityType = "Aircraft",
            EntityId = "ABC123",
            Text = "Test comment"
        };
    }
    
    [Benchmark]
    public async Task AppendEvent()
    {
        await _store.AppendAsync(_event);
    }
    
    [Benchmark]
    public async Task LoadAllEvents()
    {
        await _store.GetAllAsync();
    }
}

// Run benchmarks
// dotnet run -c Release --project Benchmarks
```

### Profiling

**Use .NET profiling tools**:

```powershell
# CPU profiling
dotnet-trace collect --process-id <PID>

# Memory profiling
dotnet-dump collect --process-id <PID>
dotnet-dump analyze dump.dmp

# Performance counters
dotnet-counters monitor --process-id <PID>
```

---

## Performance Monitoring

### Metrics to Track

```csharp
public class PerformanceMetrics
{
    private readonly Stopwatch _stopwatch = new();
    
    public void RecordApiCall(TimeSpan duration)
    {
        ApiCallDuration.Record(duration.TotalMilliseconds);
    }
    
    public void RecordEventCount(int count)
    {
        EventCount.Add(count);
    }
    
    public void RecordMemoryUsage()
    {
        var memory = GC.GetTotalMemory(false);
        MemoryUsage.Record(memory);
    }
}
```

### Performance Dashboard

```csharp
public class PerformanceDashboard
{
    public void Display()
    {
        Console.WriteLine("=== Performance Metrics ===");
        Console.WriteLine($"Events: {_eventStore.GetCountAsync().Result}");
        Console.WriteLine($"Memory: {FormatBytes(GC.GetTotalMemory(false))}");
        Console.WriteLine($"CPU: {GetCpuUsage()}%");
        Console.WriteLine($"Last API Call: {_lastApiCallDuration}ms");
    }
}
```

---

## Performance Best Practices

### General Guidelines

1. **Profile Before Optimizing**: Measure to find bottlenecks
2. **Optimize Hot Paths**: Focus on frequently executed code
3. **Use Appropriate Data Structures**: Hash sets for lookups, lists for iterations
4. **Minimize Allocations**: Reuse objects when possible
5. **Async I/O**: Always use async for file/network operations
6. **Batch Operations**: Group operations when possible
7. **Cache Expensive Computations**: Don't recompute the same result
8. **Avoid Premature Optimization**: Readable code first, optimize later
9. **Monitor Production**: Track performance metrics in production
10. **Regular Performance Testing**: Include performance tests in CI/CD

### Code Review Checklist

- [ ] No synchronous I/O operations
- [ ] Appropriate use of async/await
- [ ] Efficient data structures chosen
- [ ] No unnecessary allocations in loops
- [ ] Proper disposal of resources
- [ ] No excessive locking
- [ ] Queries use indexes when available
- [ ] Large datasets are paginated
- [ ] Caching used for expensive operations
- [ ] Performance tests included

---

## Scalability

### Handling Growth

**Event Store Size**:
- **< 10K events**: No optimization needed
- **< 100K events**: Implement snapshots
- **< 1M events**: Archive old events
- **> 1M events**: Consider database backend

**Aircraft Count**:
- **< 500 aircraft**: Current implementation sufficient
- **< 5000 aircraft**: Optimize projections
- **> 5000 aircraft**: Consider filtering/paging

**Polling Frequency**:
- **Every 60s**: Minimal resource usage
- **Every 30s**: Recommended for production
- **Every 10s**: High resource usage, fresh data
- **< 10s**: Not recommended (API abuse)

---

## Future Optimizations

1. **Database Backend**: Replace file-based storage with SQLite/PostgreSQL
2. **Caching Layer**: Redis for distributed caching
3. **Message Queue**: RabbitMQ for event processing
4. **Horizontal Scaling**: Multiple instances with shared storage
5. **CDN**: For static content (if web interface added)
6. **Read Replicas**: Separate read and write databases
7. **Sharding**: Partition data by region/airline
8. **Load Balancing**: Distribute traffic across instances

---

*Last Updated: January 24, 2026*
