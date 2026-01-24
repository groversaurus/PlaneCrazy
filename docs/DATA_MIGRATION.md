# PlaneCrazy Data Migration Guide

## Overview

This document covers strategies for migrating data, handling event schema evolution, and managing breaking changes in the PlaneCrazy event-sourced system.

## Event Schema Evolution

### Challenge

In event sourcing, events are immutable and stored permanently. When business requirements change, we need to evolve event schemas while maintaining compatibility with historical data.

### Versioning Strategies

#### 1. Additive Changes (Preferred)

Add new optional fields to existing events.

**Example**:
```csharp
// V1 (Original)
public class CommentAdded : DomainEvent
{
    public required string EntityType { get; set; }
    public required string EntityId { get; set; }
    public required string Text { get; set; }
}

// V2 (With optional sentiment field)
public class CommentAdded : DomainEvent
{
    public required string EntityType { get; set; }
    public required string EntityId { get; set; }
    public required string Text { get; set; }
    public string? Sentiment { get; set; } // New optional field
    public double? SentimentScore { get; set; } // New optional field
}
```

**Benefits**:
- ✅ Backward compatible
- ✅ Old events still deserialize
- ✅ No data migration required

**Application Logic**:
```csharp
protected override void Apply(DomainEvent @event)
{
    if (@event is CommentAdded added)
    {
        _text = added.Text;
        
        // Handle new optional field
        _sentiment = added.Sentiment ?? "Neutral"; // Default if not present
        _sentimentScore = added.SentimentScore ?? 0.0;
    }
}
```

---

#### 2. Event Upcasting

Transform old event versions to new versions when loading.

**Implementation**:
```csharp
public interface IEventUpcaster
{
    bool CanUpcast(string eventType, int version);
    DomainEvent Upcast(DomainEvent @event);
}

public class CommentAddedUpcaster : IEventUpcaster
{
    public bool CanUpcast(string eventType, int version)
    {
        return eventType == "CommentAdded" && version < 2;
    }
    
    public DomainEvent Upcast(DomainEvent @event)
    {
        if (@event is CommentAddedV1 v1)
        {
            // Transform V1 to V2
            return new CommentAddedV2
            {
                Id = v1.Id,
                OccurredAt = v1.OccurredAt,
                EntityType = v1.EntityType,
                EntityId = v1.EntityId,
                Text = v1.Text,
                Sentiment = AnalyzeSentiment(v1.Text), // Compute from old data
                SentimentScore = ComputeScore(v1.Text)
            };
        }
        
        return @event;
    }
}
```

**Usage in Event Store**:
```csharp
public class EventStore
{
    private readonly IEnumerable<IEventUpcaster> _upcasters;
    
    public async Task<IEnumerable<DomainEvent>> GetAllAsync()
    {
        var events = await LoadEventsFromDisk();
        
        // Apply upcasters
        return events.Select(e =>
        {
            foreach (var upcaster in _upcasters)
            {
                if (upcaster.CanUpcast(e.EventType, e.Version))
                {
                    e = upcaster.Upcast(e);
                }
            }
            return e;
        });
    }
}
```

**Benefits**:
- ✅ Transparent to application code
- ✅ Historical events automatically upgraded
- ✅ No physical data migration

**Drawbacks**:
- ❌ Processing overhead on every load
- ❌ Complex upcasting logic can be error-prone

---

#### 3. Event Replacement

Create new event type, deprecate old one.

**Example**:
```csharp
// Old event (deprecated)
[Obsolete("Use CommentAddedV2 instead")]
public class CommentAdded : DomainEvent
{
    public required string Text { get; set; }
}

// New event type
public class CommentAddedV2 : DomainEvent
{
    public required string Text { get; set; }
    public required string Language { get; set; }
    public string? Sentiment { get; set; }
}
```

**Application Logic**:
```csharp
protected override void Apply(DomainEvent @event)
{
    switch (@event)
    {
        case CommentAddedV2 v2:
            ApplyV2(v2);
            break;
            
        case CommentAdded v1:
            // Handle legacy event
            ApplyLegacy(v1);
            break;
    }
}
```

**Benefits**:
- ✅ Clean separation between versions
- ✅ Clear deprecation path

**Drawbacks**:
- ❌ Dual handling logic
- ❌ Old events never upgraded

---

#### 4. Event Copy Transform

One-time migration to new event format.

**Process**:
1. Create migration script
2. Read all old events
3. Transform to new format
4. Write new events
5. Archive old events

**Example Script**:
```csharp
public class EventMigrationService
{
    public async Task MigrateCommentAddedEvents()
    {
        var oldEvents = await _eventStore.GetByTypeAsync("CommentAdded");
        var newEvents = new List<DomainEvent>();
        
        foreach (var oldEvent in oldEvents.Cast<CommentAddedV1>())
        {
            // Transform to V2
            var newEvent = new CommentAddedV2
            {
                Id = Guid.NewGuid(), // New event ID
                OccurredAt = oldEvent.OccurredAt, // Preserve timestamp
                EntityType = oldEvent.EntityType,
                EntityId = oldEvent.EntityId,
                Text = oldEvent.Text,
                Language = DetectLanguage(oldEvent.Text),
                Sentiment = AnalyzeSentiment(oldEvent.Text)
            };
            
            newEvents.Add(newEvent);
        }
        
        // Archive old events
        await ArchiveEvents(oldEvents);
        
        // Write new events
        await _eventStore.AppendBatchAsync(newEvents);
        
        // Rebuild projections
        await _projectionRebuilder.RebuildAllAsync();
    }
}
```

**Benefits**:
- ✅ Clean event store
- ✅ No legacy handling in application
- ✅ One-time cost

**Drawbacks**:
- ❌ Requires downtime
- ❌ Complex migration logic
- ❌ Risk of data loss if migration fails

---

## Migration Patterns

### Pattern 1: Backward-Compatible Expansion

**When to Use**: Adding new features without breaking existing functionality

**Steps**:
1. Add new optional fields to events
2. Update application code to handle new fields
3. Deploy application
4. New events will include new fields
5. Old events still work (fields are null/default)

**Example**: Adding sentiment analysis to comments

---

### Pattern 2: Parallel Event Types

**When to Use**: Major schema changes, maintaining both versions temporarily

**Steps**:
1. Create new event type (e.g., `CommentAddedV2`)
2. Update command handlers to emit new event type
3. Update projections to handle both event types
4. Deploy application
5. Monitor for any old events
6. After grace period, remove old event handling

**Example**: Restructuring comment events

---

### Pattern 3: Event Stream Migration

**When to Use**: Breaking changes requiring full migration

**Steps**:
1. **Prepare**: Backup event store
2. **Analyze**: Identify all affected events
3. **Develop**: Create migration scripts
4. **Test**: Test migration on copy of production data
5. **Schedule**: Plan maintenance window
6. **Execute**: Run migration
7. **Verify**: Validate data integrity
8. **Rebuild**: Rebuild all projections
9. **Deploy**: Deploy updated application

---

## Projection Migration

### Rebuilding Projections

**When Needed**:
- Projection schema changes
- Bug fixes in projection logic
- Adding new projections

**Process**:
```csharp
public class ProjectionRebuilder
{
    private readonly IEventStore _eventStore;
    private readonly IEnumerable<IProjection> _projections;
    
    public async Task RebuildAllAsync()
    {
        Console.WriteLine("Starting projection rebuild...");
        
        // 1. Clear existing projection data
        foreach (var projection in _projections)
        {
            await projection.ClearAsync();
        }
        
        // 2. Load all events
        var events = await _eventStore.GetAllAsync();
        Console.WriteLine($"Loaded {events.Count()} events");
        
        // 3. Replay events through projections
        foreach (var @event in events.OrderBy(e => e.OccurredAt))
        {
            foreach (var projection in _projections)
            {
                await projection.ApplyAsync(@event);
            }
        }
        
        // 4. Save projection state
        foreach (var projection in _projections)
        {
            await projection.SaveAsync();
        }
        
        Console.WriteLine("Projection rebuild complete");
    }
}
```

**Usage**:
```powershell
# Rebuild projections manually
dotnet run -- --rebuild-projections

# Or trigger via code
var rebuilder = serviceProvider.GetRequiredService<ProjectionRebuilder>();
await rebuilder.RebuildAllAsync();
```

---

### Incremental Projection Updates

For large event stores, rebuild in chunks:

```csharp
public async Task RebuildIncrementallyAsync(int batchSize = 1000)
{
    var lastProcessedEvent = DateTime.MinValue;
    
    while (true)
    {
        var events = await _eventStore.GetEventsAfterAsync(
            lastProcessedEvent, batchSize);
        
        if (!events.Any())
            break;
        
        foreach (var @event in events)
        {
            foreach (var projection in _projections)
            {
                await projection.ApplyAsync(@event);
            }
            lastProcessedEvent = @event.OccurredAt;
        }
        
        Console.WriteLine($"Processed up to {lastProcessedEvent}");
    }
}
```

---

## Repository Migration

### Schema Changes in Repositories

**Example**: Add new field to `Comment` entity

**Before**:
```csharp
public class Comment
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**After**:
```csharp
public class Comment
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Sentiment { get; set; } // New field
}
```

**Migration Strategy**:

**Option 1**: Additive (No Migration Needed)
```csharp
// Old JSON files will deserialize with Sentiment = null
// New entities will have Sentiment populated
```

**Option 2**: Data Backfill
```csharp
public async Task BackfillSentimentAsync()
{
    var comments = await _repository.GetAllAsync();
    
    foreach (var comment in comments.Where(c => c.Sentiment == null))
    {
        comment.Sentiment = _sentimentAnalyzer.Analyze(comment.Text);
        await _repository.UpdateAsync(comment);
    }
}
```

---

## Event Store Migration

### File-Based Event Store

**Current Structure**:
```
Documents/PlaneCrazy/Events/
├── 20260124143022456_3fa85f64-5717-4562-b3fc-2c963f66afa6.json
├── 20260124143023789_7c9e6679-7425-40de-944b-e07fc1f90ae7.json
└── ...
```

**Migration Scenarios**:

#### 1. Change Event Format

**From**: Individual JSON files  
**To**: Batched event files

```csharp
public async Task MigrateToBatchedFormat()
{
    var events = await LoadAllIndividualEvents();
    var batchedEvents = events.GroupBy(e => e.OccurredAt.Date)
        .Select(g => new EventBatch
        {
            Date = g.Key,
            Events = g.ToList()
        });
    
    foreach (var batch in batchedEvents)
    {
        var filename = $"events_{batch.Date:yyyyMMdd}.json";
        await File.WriteAllTextAsync(filename, JsonSerializer.Serialize(batch));
    }
}
```

#### 2. Compress Old Events

```csharp
public async Task CompressOldEvents(DateTime before)
{
    var oldEvents = Directory.GetFiles(eventsPath, "*.json")
        .Where(f => File.GetCreationTime(f) < before);
    
    foreach (var file in oldEvents)
    {
        var compressedFile = file + ".gz";
        using var input = File.OpenRead(file);
        using var output = File.Create(compressedFile);
        using var gzip = new GZipStream(output, CompressionMode.Compress);
        await input.CopyToAsync(gzip);
        
        File.Delete(file); // Remove uncompressed
    }
}
```

---

## Breaking Changes

### Handling Breaking Changes

**Example**: Renaming `Icao24` to `AircraftId`

**Step 1**: Create mapping
```csharp
[JsonPropertyName("icao24")]
public string Icao24 
{ 
    get => AircraftId; 
    set => AircraftId = value; 
}

public string AircraftId { get; set; }
```

**Step 2**: Update code to use new name
```csharp
// Old: aircraft.Icao24
// New: aircraft.AircraftId
```

**Step 3**: Maintain both during transition
```csharp
public class AircraftFavourited : DomainEvent
{
    [JsonPropertyName("icao24")]
    [Obsolete("Use AircraftId instead")]
    public string? Icao24 
    { 
        get => AircraftId; 
        set => AircraftId = value ?? string.Empty; 
    }
    
    [JsonPropertyName("aircraftId")]
    public string AircraftId { get; set; } = string.Empty;
}
```

**Step 4**: Migrate event store
```csharp
public async Task RenameFieldInEvents()
{
    foreach (var eventFile in Directory.GetFiles(eventsPath, "*.json"))
    {
        var json = await File.ReadAllTextAsync(eventFile);
        
        // Replace old field name with new
        json = json.Replace("\"icao24\":", "\"aircraftId\":");
        
        await File.WriteAllTextAsync(eventFile, json);
    }
}
```

**Step 5**: Remove compatibility layer after migration

---

## Data Validation

### Post-Migration Validation

```csharp
public class MigrationValidator
{
    public async Task<ValidationResult> ValidateMigrationAsync()
    {
        var result = new ValidationResult();
        
        // 1. Verify event count unchanged
        var beforeCount = await GetEventCountBeforeMigration();
        var afterCount = await _eventStore.GetCountAsync();
        result.EventCountMatch = (beforeCount == afterCount);
        
        // 2. Verify no orphaned references
        var orphanedComments = await FindOrphanedComments();
        result.OrphanedComments = orphanedComments;
        
        // 3. Verify projection consistency
        var projectionValid = await ValidateProjectionIntegrity();
        result.ProjectionIntegrity = projectionValid;
        
        // 4. Verify data types
        var typeErrors = await FindDataTypeErrors();
        result.TypeErrors = typeErrors;
        
        return result;
    }
    
    private async Task<IEnumerable<Comment>> FindOrphanedComments()
    {
        var comments = await _commentRepository.GetAllAsync();
        var validEntities = await GetValidEntityIdsAsync();
        
        return comments.Where(c => !validEntities.Contains(c.EntityId));
    }
}
```

---

## Rollback Procedures

### Event Store Rollback

```csharp
public class EventStoreRollback
{
    public async Task RollbackToBackupAsync(string backupPath)
    {
        // 1. Stop application
        Console.WriteLine("Stop the application before rolling back!");
        
        // 2. Archive current state
        var archivePath = $"Archive_{DateTime.Now:yyyyMMdd_HHmmss}";
        Directory.Move(eventsPath, archivePath);
        
        // 3. Restore from backup
        CopyDirectory(backupPath, eventsPath);
        
        // 4. Rebuild projections
        await RebuildProjectionsAsync();
        
        Console.WriteLine("Rollback complete. You can now restart the application.");
    }
}
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Backup**: Full backup of data directory
- [ ] **Document**: Document migration plan
- [ ] **Test**: Test migration on copy of production data
- [ ] **Validate**: Prepare validation scripts
- [ ] **Schedule**: Plan maintenance window
- [ ] **Notify**: Notify users of downtime

### During Migration

- [ ] **Stop Services**: Stop application/background services
- [ ] **Verify Backup**: Confirm backup is complete and valid
- [ ] **Run Migration**: Execute migration scripts
- [ ] **Monitor**: Watch for errors and log output
- [ ] **Validate**: Run validation scripts
- [ ] **Document Issues**: Log any problems encountered

### Post-Migration

- [ ] **Verify Data**: Check data integrity
- [ ] **Rebuild Projections**: Ensure projections are current
- [ ] **Start Services**: Restart application
- [ ] **Monitor**: Watch logs for errors
- [ ] **Smoke Test**: Test critical functionality
- [ ] **Document**: Document what was done
- [ ] **Notify**: Inform users migration is complete

---

## Best Practices

1. **Always Backup**: Before any migration, create full backup
2. **Test First**: Test migration on non-production data
3. **Version Events**: Include version field in event schema
4. **Document Changes**: Maintain migration log
5. **Prefer Additive**: Use additive changes when possible
6. **Validate**: Always validate data after migration
7. **Plan Rollback**: Have rollback procedure ready
8. **Monitor**: Watch for issues after deployment
9. **Gradual Changes**: Break large migrations into smaller steps
10. **Preserve History**: Never delete historical events unless required

---

## Future Enhancements

1. **Migration Framework**: Built-in migration infrastructure
2. **Version Tracking**: Automatic event versioning
3. **Schema Registry**: Central schema management
4. **Migration History**: Track all migrations
5. **Automated Validation**: Comprehensive validation framework
6. **Blue-Green Migrations**: Zero-downtime migrations
7. **Event Transformation**: DSL for event transformations

---

*Last Updated: January 24, 2026*
