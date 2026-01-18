namespace PlaneCrazy.Domain.Events;

/// <summary>
/// Event raised to record the last time an aircraft was seen.
/// This is useful for tracking when aircraft disappear from tracking.
/// </summary>
public class AircraftLastSeen : DomainEvent
{
    /// <summary>
    /// The ICAO24 hex identifier of the aircraft.
    /// </summary>
    public required string Icao24 { get; init; }
    
    /// <summary>
    /// When the aircraft was last seen.
    /// </summary>
    public DateTime LastSeenAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last known latitude.
    /// </summary>
    public double? LastLatitude { get; init; }
    
    /// <summary>
    /// Last known longitude.
    /// </summary>
    public double? LastLongitude { get; init; }
    
    /// <summary>
    /// Last known altitude in feet.
    /// </summary>
    public double? LastAltitude { get; init; }
    
    public override string? GetEntityType() => "Aircraft";
    public override string? GetEntityId() => Icao24;
}
