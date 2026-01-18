using Microsoft.Extensions.Logging;
using PlaneCrazy.Domain.Aggregates;
using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Domain.Validation;
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
    private readonly ILogger<FavouriteAircraftTypeCommandHandler>? _logger;

    public FavouriteAircraftTypeCommandHandler(
        IEventStore eventStore, 
        FavouriteProjection favouriteProjection,
        ILogger<FavouriteAircraftTypeCommandHandler>? logger = null)
    {
        _eventStore = eventStore;
        _favouriteProjection = favouriteProjection;
        _logger = logger;
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
        _logger?.LogInformation("Handling FavouriteAircraftType command for {TypeCode}", 
            command.TypeCode);
        
        try
        {
            // Step 1: Validate the command
            command.Validate();
            _logger?.LogDebug("Command validation passed");

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
            
            _logger?.LogInformation("Successfully handled FavouriteAircraftType command for {TypeCode}", 
                command.TypeCode);
        }
        catch (ValidationException ex)
        {
            _logger?.LogWarning(ex, "Validation failed for command: {Errors}", 
                string.Join(", ", ex.ValidationErrors));
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger?.LogWarning(ex, "Validation failed for FavouriteAircraftType command");
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling FavouriteAircraftType command for {TypeCode}", 
                command.TypeCode);
            throw;
        }
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
