using System.Diagnostics;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Domain.Models;
using Microsoft.Extensions.Logging;

namespace PlaneCrazy.Infrastructure.EventDispatcher;

/// <summary>
/// Coordinates event writing to the event store and updates to projections.
/// Provides centralized event publishing with error handling and logging.
/// </summary>
public class EventDispatcher : IEventDispatcher
{
    private readonly IEventStore _eventStore;
    private readonly IEnumerable<IProjection> _projections;
    private readonly ILogger<EventDispatcher>? _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public EventDispatcher(
        IEventStore eventStore,
        IEnumerable<IProjection> projections,
        ILogger<EventDispatcher>? logger = null)
    {
        _eventStore = eventStore;
        _projections = projections;
        _logger = logger;
    }

    /// <summary>
    /// Publishes an event: writes it to the event store and updates all relevant projections.
    /// </summary>
    public async Task<EventDispatchResult> DispatchAsync(DomainEvent domainEvent)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new EventDispatchResult
        {
            EventId = domainEvent.Id,
            EventType = domainEvent.EventType,
            Success = false
        };

        await _semaphore.WaitAsync();
        try
        {
            _logger?.LogDebug("Dispatching event {EventType} with ID {EventId}", 
                domainEvent.EventType, domainEvent.Id);

            // Step 1: Write event to event store
            var writeStopwatch = Stopwatch.StartNew();
            try
            {
                await _eventStore.AppendAsync(domainEvent);
                result.EventStoreWriteTimeMs = writeStopwatch.ElapsedMilliseconds;
                _logger?.LogDebug("Event {EventId} written to event store in {Ms}ms", 
                    domainEvent.Id, writeStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                result.Error = $"Failed to write event to store: {ex.Message}";
                result.Exception = ex;
                _logger?.LogError(ex, "Failed to write event {EventId} to event store", domainEvent.Id);
                return result;
            }

            // Step 2: Update all projections
            var projectionResults = new List<ProjectionUpdateResult>();
            
            foreach (var projection in _projections)
            {
                var projectionStopwatch = Stopwatch.StartNew();
                var projectionResult = new ProjectionUpdateResult
                {
                    ProjectionName = projection.ProjectionName
                };

                try
                {
                    var handled = await projection.ApplyEventAsync(domainEvent);
                    projectionResult.Success = true;
                    projectionResult.EventHandled = handled;
                    projectionResult.UpdateTimeMs = projectionStopwatch.ElapsedMilliseconds;
                    
                    _logger?.LogDebug("Projection {Projection} {Status} event {EventId} in {Ms}ms",
                        projection.ProjectionName,
                        handled ? "handled" : "ignored",
                        domainEvent.Id,
                        projectionStopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    projectionResult.Success = false;
                    projectionResult.Error = ex.Message;
                    projectionResult.Exception = ex;
                    
                    _logger?.LogError(ex, "Projection {Projection} failed to apply event {EventId}",
                        projection.ProjectionName, domainEvent.Id);
                }

                projectionResults.Add(projectionResult);
            }

            result.ProjectionResults = projectionResults;
            result.Success = projectionResults.All(r => r.Success);
            result.TotalTimeMs = stopwatch.ElapsedMilliseconds;

            if (result.Success)
            {
                _logger?.LogInformation("Successfully dispatched event {EventType} ({EventId}) in {Ms}ms",
                    domainEvent.EventType, domainEvent.Id, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                var failedProjections = projectionResults.Where(r => !r.Success).Select(r => r.ProjectionName);
                _logger?.LogWarning("Event {EventId} dispatched with projection failures: {Projections}",
                    domainEvent.Id, string.Join(", ", failedProjections));
            }

            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Publishes multiple events in sequence.
    /// </summary>
    public async Task<BatchDispatchResult> DispatchBatchAsync(IEnumerable<DomainEvent> events)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<EventDispatchResult>();
        
        _logger?.LogInformation("Dispatching batch of {Count} events", events.Count());

        foreach (var @event in events)
        {
            var result = await DispatchAsync(@event);
            results.Add(result);

            // Stop if critical error (event store write failed)
            if (!result.Success && result.Exception != null && !string.IsNullOrEmpty(result.Error))
            {
                _logger?.LogError("Batch dispatch stopped due to critical error at event {EventId}", @event.Id);
                break;
            }
        }

        var batchResult = new BatchDispatchResult
        {
            TotalEvents = events.Count(),
            SuccessfulEvents = results.Count(r => r.Success),
            FailedEvents = results.Count(r => !r.Success),
            EventResults = results,
            TotalTimeMs = stopwatch.ElapsedMilliseconds
        };

        _logger?.LogInformation("Batch dispatch complete: {Successful}/{Total} successful in {Ms}ms",
            batchResult.SuccessfulEvents, batchResult.TotalEvents, batchResult.TotalTimeMs);

        return batchResult;
    }

    /// <summary>
    /// Gets statistics about registered projections.
    /// </summary>
    public ProjectionStatistics GetProjectionStatistics()
    {
        return new ProjectionStatistics
        {
            TotalProjections = _projections.Count(),
            ProjectionNames = _projections.Select(p => p.ProjectionName).ToList()
        };
    }
}
