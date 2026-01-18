using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Models;

namespace PlaneCrazy.Domain.Interfaces;

/// <summary>
/// Interface for dispatching events to the event store and projections.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches a single event to the event store and all projections.
    /// </summary>
    Task<EventDispatchResult> DispatchAsync(DomainEvent domainEvent);
    
    /// <summary>
    /// Dispatches multiple events in sequence.
    /// </summary>
    Task<BatchDispatchResult> DispatchBatchAsync(IEnumerable<DomainEvent> events);
    
    /// <summary>
    /// Gets information about registered projections.
    /// </summary>
    ProjectionStatistics GetProjectionStatistics();
}
