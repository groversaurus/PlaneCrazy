using PlaneCrazy.Domain.Aggregates;
using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.Projections;

namespace PlaneCrazy.Infrastructure.CommandHandlers;

/// <summary>
/// Command handler for favouriting aircraft types.
/// Loads event stream, rebuilds aggregate state, validates command, emits events, and updates projections.
/// </summary>
public class FavouriteAircraftTypeCommandHandler : ICommandHandler<FavouriteAircraftTypeCommand>
{
    private readonly IEventStore _eventStore;
    private readonly FavouriteProjection _favouriteProjection;

    public FavouriteAircraftTypeCommandHandler(IEventStore eventStore, FavouriteProjection favouriteProjection)
    {
        _eventStore = eventStore;
        _favouriteProjection = favouriteProjection;
    }

    /// <summary>
    /// Handles the FavouriteAircraftType command by:
    /// 1. Validating the command
    /// 2. Loading the type's event stream from the event store
    /// 3. Rebuilding the favourite aggregate state from historical events
    /// 4. Executing the command on the aggregate (which validates business rules and generates new events)
    /// 5. Persisting the new events to the event store
    /// 6. Updating the favourite projection
    /// </summary>
    public async Task HandleAsync(FavouriteAircraftTypeCommand command)
    {
        // Step 1: Validate the command
        command.Validate();

        // Step 2: Load all events for this type favourite from the event store
        var entityType = "Type";
        var entityId = command.TypeCode;
        
        var allEvents = await _eventStore.ReadAllEventsAsync();
        var favouriteEvents = allEvents
            .Where(e => IsTypeFavouriteEvent(e, command.TypeCode))
            .OrderBy(e => e.OccurredAt)
            .ToList();

        // Step 3: Rebuild the aggregate state from the event stream
        var aggregate = new FavouriteAggregate(entityType, entityId);
        aggregate.LoadFromHistory(favouriteEvents);

        // Step 4: Execute the command on the aggregate
        // This validates business rules and generates new domain events
        aggregate.FavouriteAircraftType(command);

        // Step 5: Persist all uncommitted events to the event store
        var events = aggregate.GetUncommittedEvents();
        foreach (var @event in events)
        {
            await _eventStore.AppendEventAsync(@event);
        }

        // Mark events as committed
        aggregate.MarkEventsAsCommitted();

        // Step 6: Update the favourite projection (read model)
        await _favouriteProjection.RebuildAsync();
    }

    /// <summary>
    /// Determines if an event is related to a specific type favourite.
    /// </summary>
    private bool IsTypeFavouriteEvent(DomainEvent @event, string typeCode)
    {
        return @event switch
        {
            TypeFavourited favourited => favourited.TypeCode == typeCode,
            TypeUnfavourited unfavourited => unfavourited.TypeCode == typeCode,
            _ => false
        };
    }
}
