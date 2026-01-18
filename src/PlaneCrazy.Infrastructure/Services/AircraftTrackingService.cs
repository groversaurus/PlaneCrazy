using PlaneCrazy.Domain.Entities;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.Repositories;

namespace PlaneCrazy.Infrastructure.Services;

/// <summary>
/// Service for tracking aircraft and emitting state change events.
/// </summary>
public class AircraftTrackingService
{
    private readonly IEventStore _eventStore;
    private readonly AircraftRepository _aircraftRepository;

    public AircraftTrackingService(IEventStore eventStore, AircraftRepository aircraftRepository)
    {
        _eventStore = eventStore;
        _aircraftRepository = aircraftRepository;
    }

    /// <summary>
    /// Tracks an aircraft update by emitting appropriate events.
    /// </summary>
    public async Task TrackAircraftAsync(Aircraft aircraft)
    {
        var existing = await _aircraftRepository.GetByIdAsync(aircraft.Icao24);
        
        if (existing == null)
        {
            // First time seeing this aircraft
            await EmitFirstSeenEventAsync(aircraft);
        }
        
        // Emit position update if position data changed
        if (HasPositionChanged(existing, aircraft))
        {
            await EmitPositionUpdateEventAsync(aircraft);
        }
        
        // Emit identity update if identity data changed
        if (HasIdentityChanged(existing, aircraft))
        {
            await EmitIdentityUpdateEventAsync(aircraft);
        }
        
        // Always emit last seen
        await EmitLastSeenEventAsync(aircraft);
    }

    private async Task EmitFirstSeenEventAsync(Aircraft aircraft)
    {
        var @event = new AircraftFirstSeen
        {
            Icao24 = aircraft.Icao24,
            FirstSeenAt = DateTime.UtcNow,
            InitialLatitude = aircraft.Latitude,
            InitialLongitude = aircraft.Longitude
        };
        
        await _eventStore.AppendAsync(@event);
    }

    private async Task EmitPositionUpdateEventAsync(Aircraft aircraft)
    {
        var @event = new AircraftPositionUpdated
        {
            Icao24 = aircraft.Icao24,
            Latitude = aircraft.Latitude,
            Longitude = aircraft.Longitude,
            Altitude = aircraft.Altitude,
            Velocity = aircraft.Velocity,
            Track = aircraft.Track,
            VerticalRate = aircraft.VerticalRate,
            OnGround = aircraft.OnGround,
            Timestamp = DateTime.UtcNow
        };
        
        await _eventStore.AppendAsync(@event);
    }

    private async Task EmitIdentityUpdateEventAsync(Aircraft aircraft)
    {
        var @event = new AircraftIdentityUpdated
        {
            Icao24 = aircraft.Icao24,
            Registration = aircraft.Registration,
            TypeCode = aircraft.TypeCode,
            Callsign = aircraft.Callsign,
            Squawk = aircraft.Squawk,
            Origin = aircraft.Origin,
            Destination = aircraft.Destination,
            Timestamp = DateTime.UtcNow
        };
        
        await _eventStore.AppendAsync(@event);
    }

    private async Task EmitLastSeenEventAsync(Aircraft aircraft)
    {
        var @event = new AircraftLastSeen
        {
            Icao24 = aircraft.Icao24,
            LastSeenAt = DateTime.UtcNow,
            LastLatitude = aircraft.Latitude,
            LastLongitude = aircraft.Longitude,
            LastAltitude = aircraft.Altitude
        };
        
        await _eventStore.AppendAsync(@event);
    }

    private bool HasPositionChanged(Aircraft? existing, Aircraft updated)
    {
        if (existing == null) return true;
        
        return existing.Latitude != updated.Latitude
            || existing.Longitude != updated.Longitude
            || existing.Altitude != updated.Altitude
            || existing.Velocity != updated.Velocity
            || existing.Track != updated.Track;
    }

    private bool HasIdentityChanged(Aircraft? existing, Aircraft updated)
    {
        if (existing == null) return true;
        
        return existing.Registration != updated.Registration
            || existing.TypeCode != updated.TypeCode
            || existing.Callsign != updated.Callsign;
    }
}
