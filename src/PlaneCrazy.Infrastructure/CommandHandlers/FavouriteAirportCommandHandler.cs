using Microsoft.Extensions.Logging;
using PlaneCrazy.Domain.Aggregates;
using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Domain.Validation;
using PlaneCrazy.Infrastructure.Projections;

namespace PlaneCrazy.Infrastructure.CommandHandlers;

/// <summary>
/// Command handler for favouriting airports.
/// Loads event stream, rebuilds aggregate state, validates command, emits events, and updates projections.
/// </summary>
public class FavouriteAirportCommandHandler : ICommandHandler<FavouriteAirportCommand>
{
    private readonly IEventStore _eventStore;
    private readonly FavouriteProjection _favouriteProjection;
    private readonly ILogger<FavouriteAirportCommandHandler>? _logger;

    public FavouriteAirportCommandHandler(
        IEventStore eventStore, 
        FavouriteProjection favouriteProjection,
        ILogger<FavouriteAirportCommandHandler>? logger = null)
    {
        _eventStore = eventStore;
        _favouriteProjection = favouriteProjection;
        _logger = logger;
    }

    /// <summary>
    /// Handles the FavouriteAirport command by:
    /// 1. Validating the command
    /// 2. Loading the airport's event stream from the event store
    /// 3. Rebuilding the favourite aggregate state from historical events
    /// 4. Executing the command on the aggregate (which validates business rules and generates new events)
    /// 5. Persisting the new events to the event store
    /// 6. Updating the favourite projection
    /// </summary>
    public async Task HandleAsync(FavouriteAirportCommand command)
    {
        _logger?.LogInformation("Handling FavouriteAirport command for {IcaoCode}", 
            command.IcaoCode);
        
        try
        {
            // Step 1: Validate the command
            command.Validate();
            _logger?.LogDebug("Command validation passed");

            // Step 2: Load all events for this airport favourite from the event store
            var entityType = "Airport";
            var entityId = command.IcaoCode;
            
            var allEvents = await _eventStore.ReadAllEventsAsync();
            var favouriteEvents = allEvents
                .Where(e => IsAirportFavouriteEvent(e, command.IcaoCode))
                .OrderBy(e => e.OccurredAt)
                .ToList();

            // Step 3: Rebuild the aggregate state from the event stream
            var aggregate = new FavouriteAggregate(entityType, entityId);
            aggregate.LoadFromHistory(favouriteEvents);

            // Step 4: Execute the command on the aggregate
            // This validates business rules and generates new domain events
            aggregate.FavouriteAirport(command);

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
            
            _logger?.LogInformation("Successfully handled FavouriteAirport command for {IcaoCode}", 
                command.IcaoCode);
        }
        catch (ValidationException ex)
        {
            _logger?.LogWarning(ex, "Validation failed for command: {Errors}", 
                string.Join(", ", ex.ValidationErrors));
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger?.LogWarning(ex, "Validation failed for command");
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling FavouriteAirport command for {IcaoCode}", 
                command.IcaoCode);
            throw;
        }
    }

    /// <summary>
    /// Determines if an event is related to a specific airport favourite.
    /// </summary>
    private bool IsAirportFavouriteEvent(DomainEvent @event, string icaoCode)
    {
        return @event switch
        {
            AirportFavourited favourited => favourited.IcaoCode == icaoCode,
            AirportUnfavourited unfavourited => unfavourited.IcaoCode == icaoCode,
            _ => false
        };
    }
}
