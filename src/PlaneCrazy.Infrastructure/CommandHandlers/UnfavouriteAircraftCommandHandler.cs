using Microsoft.Extensions.Logging;
using PlaneCrazy.Domain.Aggregates;
using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Domain.Validation;
using PlaneCrazy.Infrastructure.Projections;

namespace PlaneCrazy.Infrastructure.CommandHandlers;

/// <summary>
/// Command handler for unfavouriting aircraft.
/// Loads event stream, rebuilds aggregate state, validates command, emits events, and updates projections.
/// </summary>
public class UnfavouriteAircraftCommandHandler : ICommandHandler<UnfavouriteAircraftCommand>
{
    private readonly IEventStore _eventStore;
    private readonly FavouriteProjection _favouriteProjection;
    private readonly ILogger<UnfavouriteAircraftCommandHandler>? _logger;

    public UnfavouriteAircraftCommandHandler(
        IEventStore eventStore, 
        FavouriteProjection favouriteProjection,
        ILogger<UnfavouriteAircraftCommandHandler>? logger = null)
    {
        _eventStore = eventStore;
        _favouriteProjection = favouriteProjection;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UnfavouriteAircraft command by:
    /// 1. Validating the command
    /// 2. Loading the aircraft's event stream from the event store
    /// 3. Rebuilding the favourite aggregate state from historical events
    /// 4. Executing the command on the aggregate (which validates business rules and generates new events)
    /// 5. Persisting the new events to the event store
    /// 6. Updating the favourite projection
    /// </summary>
    public async Task HandleAsync(UnfavouriteAircraftCommand command)
    {
        _logger?.LogInformation("Handling UnfavouriteAircraft command for {Icao24}", 
            command.Icao24);
        
        try
        {
            // Step 1: Validate the command
            command.Validate();
            _logger?.LogDebug("Command validation passed");

            // Step 2: Load all events for this aircraft favourite from the event store
            var entityType = "Aircraft";
            var entityId = command.Icao24;
            
            var allEvents = await _eventStore.ReadAllEventsAsync();
            var favouriteEvents = allEvents
                .Where(e => IsAircraftFavouriteEvent(e, command.Icao24))
                .OrderBy(e => e.OccurredAt)
                .ToList();

            // Step 3: Rebuild the aggregate state from the event stream
            var aggregate = new FavouriteAggregate(entityType, entityId);
            aggregate.LoadFromHistory(favouriteEvents);

            // Step 4: Execute the command on the aggregate
            // This validates business rules and generates new domain events
            aggregate.UnfavouriteAircraft(command);

            // Step 5: Persist all uncommitted events to the event store
            var events = aggregate.GetUncommittedEvents();
            _logger?.LogDebug("Generated {EventCount} events", events.Count());
            
            foreach (var @event in events)
            {
                await _eventStore.AppendEventAsync(@event);
                _logger?.LogDebug("Persisted event {EventType}", @event.EventType);
            }

            // Mark events as committed
            aggregate.MarkEventsAsCommitted();

            // Step 6: Update the favourite projection (read model)
            _logger?.LogDebug("Rebuilding FavouriteProjection");
            await _favouriteProjection.RebuildAsync();
            
            _logger?.LogInformation("Successfully handled UnfavouriteAircraft command for {Icao24}", 
                command.Icao24);
        }
        catch (ValidationException ex)
        {
            _logger?.LogWarning(ex, "Validation failed: {Errors}", 
                string.Join(", ", ex.ValidationErrors));
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger?.LogWarning(ex, "Validation failed for UnfavouriteAircraft command");
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling UnfavouriteAircraft command for {Icao24}", 
                command.Icao24);
            throw;
        }
    }

    /// <summary>
    /// Determines if an event is related to a specific aircraft favourite.
    /// </summary>
    private bool IsAircraftFavouriteEvent(DomainEvent @event, string icao24)
    {
        return @event switch
        {
            AircraftFavourited favourited => favourited.Icao24 == icao24,
            AircraftUnfavourited unfavourited => unfavourited.Icao24 == icao24,
            _ => false
        };
    }
}
