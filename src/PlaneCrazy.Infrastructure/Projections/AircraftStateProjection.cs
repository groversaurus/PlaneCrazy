using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.Repositories;
using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Infrastructure.Projections;

/// <summary>
/// Projection that rebuilds aircraft state from domain events.
/// Replays all aircraft-related events to reconstruct current state.
/// </summary>
public class AircraftStateProjection
{
    private readonly IEventStore _eventStore;
    private readonly AircraftRepository _aircraftRepository;

    public AircraftStateProjection(IEventStore eventStore, AircraftRepository aircraftRepository)
    {
        _eventStore = eventStore;
        _aircraftRepository = aircraftRepository;
    }

    /// <summary>
    /// Rebuilds all aircraft states from all events.
    /// </summary>
    public async Task RebuildAllAsync()
    {
        // Clear all existing aircraft
        var allAircraft = await _aircraftRepository.GetAllAsync();
        foreach (var aircraft in allAircraft)
        {
            await _aircraftRepository.DeleteAsync(aircraft.Icao24);
        }
        
        // Get all events and replay them
        var events = await _eventStore.GetAllAsync();
        var aircraftEvents = events
            .Where(e => IsAircraftEvent(e))
            .OrderBy(e => e.OccurredAt);
        
        foreach (var @event in aircraftEvents)
        {
            await ApplyEventAsync(@event);
        }
    }

    /// <summary>
    /// Rebuilds state for a specific aircraft by replaying its events.
    /// </summary>
    public async Task RebuildAircraftAsync(string icao24)
    {
        // Delete existing state
        await _aircraftRepository.DeleteAsync(icao24);
        
        // Get all events for this aircraft
        var allEvents = await _eventStore.GetAllAsync();
        var aircraftEvents = allEvents
            .Where(e => IsAircraftEvent(e) && GetAircraftIcao(e) == icao24)
            .OrderBy(e => e.OccurredAt);
        
        // Replay events in chronological order
        foreach (var @event in aircraftEvents)
        {
            await ApplyEventAsync(@event);
        }
    }

    /// <summary>
    /// Applies a single event to update the aircraft state.
    /// </summary>
    private async Task ApplyEventAsync(DomainEvent @event)
    {
        switch (@event)
        {
            case AircraftFirstSeen firstSeen:
                await HandleAircraftFirstSeenAsync(firstSeen);
                break;
                
            case AircraftPositionUpdated positionUpdated:
                await HandlePositionUpdatedAsync(positionUpdated);
                break;
                
            case AircraftIdentityUpdated identityUpdated:
                await HandleIdentityUpdatedAsync(identityUpdated);
                break;
                
            case AircraftLastSeen lastSeen:
                await HandleLastSeenAsync(lastSeen);
                break;
        }
    }

    private async Task HandleAircraftFirstSeenAsync(AircraftFirstSeen @event)
    {
        // Only create aircraft if it doesn't already exist
        var existing = await _aircraftRepository.GetByIdAsync(@event.Icao24);
        if (existing != null)
        {
            return; // Aircraft already exists, skip
        }
        
        var aircraft = new Aircraft
        {
            Icao24 = @event.Icao24,
            FirstSeen = @event.FirstSeenAt,
            LastSeen = @event.FirstSeenAt,
            LastUpdated = @event.OccurredAt,
            Latitude = @event.InitialLatitude,
            Longitude = @event.InitialLongitude,
            TotalUpdates = 0
        };
        
        await _aircraftRepository.SaveAsync(aircraft);
    }

    private async Task HandlePositionUpdatedAsync(AircraftPositionUpdated @event)
    {
        var aircraft = await GetOrCreateAircraftAsync(@event.Icao24, @event.Timestamp);
        
        // Update position data
        aircraft.Latitude = @event.Latitude;
        aircraft.Longitude = @event.Longitude;
        aircraft.Altitude = @event.Altitude;
        aircraft.Velocity = @event.Velocity;
        aircraft.Track = @event.Track;
        aircraft.VerticalRate = @event.VerticalRate;
        aircraft.OnGround = @event.OnGround;
        aircraft.LastSeen = @event.Timestamp;
        aircraft.LastUpdated = @event.OccurredAt;
        aircraft.TotalUpdates++;
        
        await _aircraftRepository.SaveAsync(aircraft);
    }

    private async Task HandleIdentityUpdatedAsync(AircraftIdentityUpdated @event)
    {
        var aircraft = await GetOrCreateAircraftAsync(@event.Icao24, @event.Timestamp);
        
        // Update identity data
        if (!string.IsNullOrEmpty(@event.Registration))
            aircraft.Registration = @event.Registration;
        
        if (!string.IsNullOrEmpty(@event.TypeCode))
            aircraft.TypeCode = @event.TypeCode;
        
        if (!string.IsNullOrEmpty(@event.Callsign))
            aircraft.Callsign = @event.Callsign;
        
        if (!string.IsNullOrEmpty(@event.Squawk))
            aircraft.Squawk = @event.Squawk;
        
        if (!string.IsNullOrEmpty(@event.Origin))
            aircraft.Origin = @event.Origin;
        
        if (!string.IsNullOrEmpty(@event.Destination))
            aircraft.Destination = @event.Destination;
        
        aircraft.LastSeen = @event.Timestamp;
        aircraft.LastUpdated = @event.OccurredAt;
        aircraft.TotalUpdates++;
        
        await _aircraftRepository.SaveAsync(aircraft);
    }

    private async Task HandleLastSeenAsync(AircraftLastSeen @event)
    {
        var aircraft = await GetOrCreateAircraftAsync(@event.Icao24, @event.LastSeenAt);
        
        aircraft.LastSeen = @event.LastSeenAt;
        aircraft.LastUpdated = @event.OccurredAt;
        
        // Update last known position if provided
        if (@event.LastLatitude.HasValue)
            aircraft.Latitude = @event.LastLatitude;
        
        if (@event.LastLongitude.HasValue)
            aircraft.Longitude = @event.LastLongitude;
        
        if (@event.LastAltitude.HasValue)
            aircraft.Altitude = @event.LastAltitude;
        
        await _aircraftRepository.SaveAsync(aircraft);
    }

    private bool IsAircraftEvent(DomainEvent @event)
    {
        return @event is AircraftFirstSeen 
            || @event is AircraftPositionUpdated 
            || @event is AircraftIdentityUpdated 
            || @event is AircraftLastSeen;
    }

    private string? GetAircraftIcao(DomainEvent @event)
    {
        return @event switch
        {
            AircraftFirstSeen firstSeen => firstSeen.Icao24,
            AircraftPositionUpdated posUpdated => posUpdated.Icao24,
            AircraftIdentityUpdated idUpdated => idUpdated.Icao24,
            AircraftLastSeen lastSeen => lastSeen.Icao24,
            _ => null
        };
    }

    private async Task<Aircraft> GetOrCreateAircraftAsync(string icao24, DateTime timestamp)
    {
        var aircraft = await _aircraftRepository.GetByIdAsync(icao24);
        
        if (aircraft == null)
        {
            // Create new aircraft if it doesn't exist
            aircraft = new Aircraft
            {
                Icao24 = icao24,
                FirstSeen = timestamp
            };
        }
        
        return aircraft;
    }
}
