namespace PlaneCrazy.Core.Models;

/// <summary>
/// Represents ADS-B aircraft data
/// </summary>
public class AircraftData
{
    /// <summary>
    /// ICAO 24-bit address (unique aircraft identifier)
    /// </summary>
    public string Icao24 { get; set; } = string.Empty;

    /// <summary>
    /// Aircraft callsign
    /// </summary>
    public string? Callsign { get; set; }

    /// <summary>
    /// Latitude in decimal degrees
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude in decimal degrees
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Altitude in feet
    /// </summary>
    public int? Altitude { get; set; }

    /// <summary>
    /// Ground speed in knots
    /// </summary>
    public double? GroundSpeed { get; set; }

    /// <summary>
    /// Heading in degrees
    /// </summary>
    public double? Heading { get; set; }

    /// <summary>
    /// Vertical rate in feet per minute
    /// </summary>
    public int? VerticalRate { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
}
