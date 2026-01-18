namespace PlaneCrazy.Domain.Events;

/// <summary>
/// Event raised when an aircraft's position is updated.
/// </summary>
public class AircraftPositionUpdated : DomainEvent
{
    /// <summary>
    /// The ICAO24 hex identifier of the aircraft.
    /// </summary>
    public required string Icao24 { get; init; }
    
    /// <summary>
    /// Latitude in decimal degrees.
    /// </summary>
    public double? Latitude { get; init; }
    
    /// <summary>
    /// Longitude in decimal degrees.
    /// </summary>
    public double? Longitude { get; init; }
    
    /// <summary>
    /// Barometric altitude in feet.
    /// </summary>
    public double? Altitude { get; init; }
    
    /// <summary>
    /// Ground speed in knots.
    /// </summary>
    public double? Velocity { get; init; }
    
    /// <summary>
    /// Track/heading in degrees (0-359).
    /// </summary>
    public double? Track { get; init; }
    
    /// <summary>
    /// Vertical rate in feet per minute.
    /// </summary>
    public double? VerticalRate { get; init; }
    
    /// <summary>
    /// Whether the aircraft is on the ground.
    /// </summary>
    public bool OnGround { get; init; }
    
    /// <summary>
    /// Timestamp of the position update.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    public override string? GetEntityType() => "Aircraft";
    public override string? GetEntityId() => Icao24;
}
