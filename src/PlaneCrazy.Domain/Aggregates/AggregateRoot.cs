using PlaneCrazy.Domain.Events;

namespace PlaneCrazy.Domain.Aggregates;

/// <summary>
/// Base class for aggregate roots in the event-sourced domain model.
/// An aggregate root is responsible for maintaining consistency and enforcing business rules.
/// It rebuilds its state from a stream of domain events and can validate commands before emitting new events.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _uncommittedEvents = new();

    /// <summary>
    /// Gets the unique identifier of the aggregate.
    /// </summary>
    public string Id { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the version of the aggregate (number of events applied).
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Gets the uncommitted events that have been applied but not yet persisted.
    /// </summary>
    public IEnumerable<DomainEvent> GetUncommittedEvents() => _uncommittedEvents;

    /// <summary>
    /// Marks all uncommitted events as committed.
    /// </summary>
    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }

    /// <summary>
    /// Loads the aggregate from a historical stream of events.
    /// </summary>
    /// <param name="history">The sequence of events to replay.</param>
    public void LoadFromHistory(IEnumerable<DomainEvent> history)
    {
        foreach (var @event in history)
        {
            ApplyEvent(@event, isNew: false);
        }
    }

    /// <summary>
    /// Applies a new event to the aggregate and records it as uncommitted.
    /// </summary>
    /// <param name="event">The event to apply.</param>
    protected void ApplyChange(DomainEvent @event)
    {
        ApplyEvent(@event, isNew: true);
    }

    /// <summary>
    /// Applies an event to the aggregate's state.
    /// </summary>
    /// <param name="event">The event to apply.</param>
    /// <param name="isNew">True if this is a new event being generated; false if replaying from history.</param>
    private void ApplyEvent(DomainEvent @event, bool isNew)
    {
        // Call the specific Apply method for the event type
        Apply(@event);
        
        if (isNew)
        {
            _uncommittedEvents.Add(@event);
        }

        Version++;
    }

    /// <summary>
    /// Override this method to apply event-specific logic to rebuild aggregate state.
    /// </summary>
    /// <param name="event">The event to apply.</param>
    protected abstract void Apply(DomainEvent @event);
}
