namespace PlaneCrazy.Domain.Events;

/// <summary>
/// Event raised when an aircraft's identity information is updated.
/// </summary>
public class AircraftIdentityUpdated : DomainEvent
{
    /// <summary>
    /// The ICAO24 hex identifier of the aircraft.
    /// </summary>
    public required string Icao24 { get; init; }
    
    /// <summary>
    /// Aircraft registration/tail number.
    /// </summary>
    public string? Registration { get; init; }
    
    /// <summary>
    /// Aircraft type code (e.g., "B738", "A320").
    /// </summary>
    public string? TypeCode { get; init; }
    
    /// <summary>
    /// Flight callsign.
    /// </summary>
    public string? Callsign { get; init; }
    
    /// <summary>
    /// Squawk code (4-digit transponder code).
    /// </summary>
    public string? Squawk { get; init; }
    
    /// <summary>
    /// Origin airport ICAO code.
    /// </summary>
    public string? Origin { get; init; }
    
    /// <summary>
    /// Destination airport ICAO code.
    /// </summary>
    public string? Destination { get; init; }
    
    /// <summary>
    /// Timestamp of the identity update.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    public override string? GetEntityType() => "Aircraft";
    public override string? GetEntityId() => Icao24;
}
