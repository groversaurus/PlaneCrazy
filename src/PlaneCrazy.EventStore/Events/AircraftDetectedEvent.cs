namespace PlaneCrazy.EventStore.Events;

/// <summary>
/// Event raised when an aircraft is detected by the ADS-B system.
/// </summary>
public class AircraftDetectedEvent : EventBase
{
    /// <summary>
    /// Gets or sets the ICAO 24-bit aircraft address.
    /// </summary>
    public string IcaoAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the aircraft callsign.
    /// </summary>
    public string? Callsign { get; set; }
    
    /// <summary>
    /// Gets or sets the initial latitude.
    /// </summary>
    public double? Latitude { get; set; }
    
    /// <summary>
    /// Gets or sets the initial longitude.
    /// </summary>
    public double? Longitude { get; set; }
    
    /// <summary>
    /// Gets or sets the altitude in feet.
    /// </summary>
    public int? Altitude { get; set; }
}
