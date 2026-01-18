namespace PlaneCrazy.Domain.Events;

/// <summary>
/// Event raised when an aircraft is first detected/tracked.
/// </summary>
public class AircraftFirstSeen : DomainEvent
{
    /// <summary>
    /// The ICAO24 hex identifier of the aircraft.
    /// </summary>
    public required string Icao24 { get; init; }
    
    /// <summary>
    /// When the aircraft was first seen.
    /// </summary>
    public DateTime FirstSeenAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Initial latitude (if available).
    /// </summary>
    public double? InitialLatitude { get; init; }
    
    /// <summary>
    /// Initial longitude (if available).
    /// </summary>
    public double? InitialLongitude { get; init; }
    
    public override string? GetEntityType() => "Aircraft";
    public override string? GetEntityId() => Icao24;
}
