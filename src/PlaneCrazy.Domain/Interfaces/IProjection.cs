using PlaneCrazy.Domain.Events;

namespace PlaneCrazy.Domain.Interfaces;

/// <summary>
/// Interface for projections that can be updated with individual events.
/// </summary>
public interface IProjection
{
    /// <summary>
    /// Applies a single event to update the projection.
    /// </summary>
    /// <param name="domainEvent">The event to apply.</param>
    /// <returns>True if the projection handled the event, false if it was ignored.</returns>
    Task<bool> ApplyEventAsync(DomainEvent domainEvent);
    
    /// <summary>
    /// Gets the name of this projection for logging/debugging.
    /// </summary>
    string ProjectionName { get; }
}
