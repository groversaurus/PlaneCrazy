namespace PlaneCrazy.EventStore;

/// <summary>
/// Represents an event store for persisting and retrieving events.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Saves an event to the event store.
    /// </summary>
    /// <param name="event">The event to save.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveEventAsync(IEvent @event);
    
    /// <summary>
    /// Gets all events for a specific entity.
    /// </summary>
    /// <param name="entityType">The type of the entity.</param>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <returns>A list of events for the entity.</returns>
    Task<IReadOnlyList<IEvent>> GetEventsAsync(string entityType, string entityId);
    
    /// <summary>
    /// Gets all events for a specific entity type.
    /// </summary>
    /// <param name="entityType">The type of the entity.</param>
    /// <returns>A list of events for the entity type.</returns>
    Task<IReadOnlyList<IEvent>> GetEventsByEntityTypeAsync(string entityType);
}
