using PlaneCrazy.Domain.Entities;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Domain.Models;

namespace PlaneCrazy.Infrastructure.Projections;

/// <summary>
/// Projection that reconstructs complete aircraft snapshots at any point in time.
/// Enables "time travel" by replaying events up to a specific timestamp.
/// </summary>
public class SnapshotProjection
{
    private readonly IEventStore _eventStore;

    public SnapshotProjection(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    /// <summary>
    /// Reconstructs the state of all aircraft at a specific point in time.
    /// </summary>
    /// <param name="asOf">The timestamp to reconstruct state at.</param>
    /// <returns>A snapshot containing all aircraft known at that time.</returns>
    public async Task<AircraftSnapshot> GetSnapshotAtAsync(DateTime asOf)
    {
        // Get all aircraft events up to the specified time
        var events = await _eventStore.ReadEventsAsync(
            fromTimestamp: null,
            toTimestamp: asOf);

        // Filter to aircraft-related events
        var aircraftEvents = events
            .Where(e => IsAircraftEvent(e))
            .OrderBy(e => e.OccurredAt)
            .ToList();

        // Build aircraft state by replaying events
        var aircraftStates = new Dictionary<string, Aircraft>();

        foreach (var @event in aircraftEvents)
        {
            ApplyEventToSnapshot(aircraftStates, @event);
        }

        // Create snapshot
        var snapshot = new AircraftSnapshot
        {
            Timestamp = asOf,
            Aircraft = aircraftStates.Values.ToList(),
            EventCount = aircraftEvents.Count,
            AircraftCount = aircraftStates.Count
        };

        return snapshot;
    }

    /// <summary>
    /// Reconstructs the state of all aircraft at the current time.
    /// </summary>
    public async Task<AircraftSnapshot> GetCurrentSnapshotAsync()
    {
        return await GetSnapshotAtAsync(DateTime.UtcNow);
    }

    /// <summary>
    /// Reconstructs snapshots at regular intervals over a time range.
    /// Useful for creating time-lapse visualizations.
    /// </summary>
    /// <param name="startTime">The start of the time range.</param>
    /// <param name="endTime">The end of the time range.</param>
    /// <param name="interval">The interval between snapshots.</param>
    /// <returns>A collection of snapshots over time.</returns>
    public async Task<IEnumerable<AircraftSnapshot>> GetSnapshotSeriesAsync(
        DateTime startTime, 
        DateTime endTime, 
        TimeSpan interval)
    {
        var snapshots = new List<AircraftSnapshot>();
        var currentTime = startTime;

        while (currentTime <= endTime)
        {
            var snapshot = await GetSnapshotAtAsync(currentTime);
            snapshots.Add(snapshot);
            currentTime = currentTime.Add(interval);
        }

        return snapshots;
    }

    /// <summary>
    /// Gets the state of a specific aircraft at a point in time.
    /// </summary>
    /// <param name="icao24">The ICAO24 identifier of the aircraft.</param>
    /// <param name="asOf">The timestamp to get state at.</param>
    /// <returns>The aircraft state at that time, or null if not known.</returns>
    public async Task<Aircraft?> GetAircraftStateAtAsync(string icao24, DateTime asOf)
    {
        // Get all events for this aircraft up to the specified time
        var events = await _eventStore.ReadEventsAsync(
            fromTimestamp: null,
            toTimestamp: asOf);

        var aircraftEvents = events
            .Where(e => IsAircraftEvent(e) && GetAircraftIcao(e) == icao24)
            .OrderBy(e => e.OccurredAt)
            .ToList();

        if (!aircraftEvents.Any())
            return null;

        // Replay events to build state
        Aircraft? aircraft = null;

        foreach (var @event in aircraftEvents)
        {
            aircraft = ApplyEventToAircraft(aircraft, @event);
        }

        return aircraft;
    }

    /// <summary>
    /// Gets statistics about aircraft activity over a time range.
    /// </summary>
    public async Task<SnapshotStatistics> GetStatisticsAsync(DateTime startTime, DateTime endTime)
    {
        var events = await _eventStore.ReadEventsAsync(
            fromTimestamp: startTime,
            toTimestamp: endTime);

        var aircraftEvents = events.Where(e => IsAircraftEvent(e)).ToList();

        var uniqueAircraft = aircraftEvents
            .Select(e => GetAircraftIcao(e))
            .Where(icao => icao != null)
            .Distinct()
            .Count();

        var firstSeenEvents = aircraftEvents.OfType<AircraftFirstSeen>().Count();
        var positionUpdates = aircraftEvents.OfType<AircraftPositionUpdated>().Count();
        var identityUpdates = aircraftEvents.OfType<AircraftIdentityUpdated>().Count();
        var lastSeenEvents = aircraftEvents.OfType<AircraftLastSeen>().Count();

        return new SnapshotStatistics
        {
            StartTime = startTime,
            EndTime = endTime,
            TotalEvents = aircraftEvents.Count,
            UniqueAircraft = uniqueAircraft,
            FirstSeenCount = firstSeenEvents,
            PositionUpdateCount = positionUpdates,
            IdentityUpdateCount = identityUpdates,
            LastSeenCount = lastSeenEvents
        };
    }

    /// <summary>
    /// Finds aircraft that were at a specific location at a specific time.
    /// </summary>
    /// <param name="latitude">Center latitude.</param>
    /// <param name="longitude">Center longitude.</param>
    /// <param name="radiusKm">Search radius in kilometers.</param>
    /// <param name="asOf">The timestamp to search at.</param>
    /// <returns>Aircraft within the radius at that time.</returns>
    public async Task<IEnumerable<Aircraft>> FindAircraftAtLocationAsync(
        double latitude, 
        double longitude, 
        double radiusKm, 
        DateTime asOf)
    {
        var snapshot = await GetSnapshotAtAsync(asOf);

        return snapshot.Aircraft.Where(a =>
        {
            if (!a.Latitude.HasValue || !a.Longitude.HasValue)
                return false;

            var distance = CalculateDistance(
                latitude, longitude,
                a.Latitude.Value, a.Longitude.Value);

            return distance <= radiusKm;
        });
    }

    /// <summary>
    /// Applies an event to update the snapshot's aircraft dictionary.
    /// </summary>
    private void ApplyEventToSnapshot(Dictionary<string, Aircraft> aircraftStates, DomainEvent @event)
    {
        var icao = GetAircraftIcao(@event);
        if (icao == null) return;

        if (!aircraftStates.ContainsKey(icao))
        {
            aircraftStates[icao] = new Aircraft { Icao24 = icao };
        }

        aircraftStates[icao] = ApplyEventToAircraft(aircraftStates[icao], @event)!;
    }

    /// <summary>
    /// Applies an event to update a single aircraft's state.
    /// </summary>
    private Aircraft? ApplyEventToAircraft(Aircraft? aircraft, DomainEvent @event)
    {
        switch (@event)
        {
            case AircraftFirstSeen firstSeen:
                return ApplyFirstSeen(aircraft, firstSeen);

            case AircraftPositionUpdated positionUpdated:
                return ApplyPositionUpdated(aircraft, positionUpdated);

            case AircraftIdentityUpdated identityUpdated:
                return ApplyIdentityUpdated(aircraft, identityUpdated);

            case AircraftLastSeen lastSeen:
                return ApplyLastSeen(aircraft, lastSeen);

            default:
                return aircraft;
        }
    }

    private Aircraft ApplyFirstSeen(Aircraft? aircraft, AircraftFirstSeen @event)
    {
        if (aircraft == null)
        {
            aircraft = new Aircraft
            {
                Icao24 = @event.Icao24
            };
        }

        aircraft.FirstSeen = @event.FirstSeenAt;
        aircraft.LastSeen = @event.FirstSeenAt;
        aircraft.Latitude = @event.InitialLatitude;
        aircraft.Longitude = @event.InitialLongitude;

        return aircraft;
    }

    private Aircraft ApplyPositionUpdated(Aircraft? aircraft, AircraftPositionUpdated @event)
    {
        if (aircraft == null)
        {
            aircraft = new Aircraft
            {
                Icao24 = @event.Icao24,
                FirstSeen = @event.Timestamp
            };
        }

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

        return aircraft;
    }

    private Aircraft ApplyIdentityUpdated(Aircraft? aircraft, AircraftIdentityUpdated @event)
    {
        if (aircraft == null)
        {
            aircraft = new Aircraft
            {
                Icao24 = @event.Icao24,
                FirstSeen = @event.Timestamp
            };
        }

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

        return aircraft;
    }

    private Aircraft ApplyLastSeen(Aircraft? aircraft, AircraftLastSeen @event)
    {
        if (aircraft == null)
        {
            aircraft = new Aircraft
            {
                Icao24 = @event.Icao24,
                FirstSeen = @event.LastSeenAt
            };
        }

        aircraft.LastSeen = @event.LastSeenAt;

        if (@event.LastLatitude.HasValue)
            aircraft.Latitude = @event.LastLatitude;

        if (@event.LastLongitude.HasValue)
            aircraft.Longitude = @event.LastLongitude;

        if (@event.LastAltitude.HasValue)
            aircraft.Altitude = @event.LastAltitude;

        return aircraft;
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

    /// <summary>
    /// Calculates the distance between two coordinates using the Haversine formula.
    /// </summary>
    /// <returns>Distance in kilometers.</returns>
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
