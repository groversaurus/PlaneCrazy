namespace PlaneCrazy.Core.Models;

/// <summary>
/// Represents an aircraft with ADS-B data.
/// </summary>
public class Aircraft
{
    /// <summary>
    /// ICAO24 address (hex code) - unique identifier for the aircraft.
    /// </summary>
    public string Hex { get; set; } = string.Empty;

    /// <summary>
    /// Flight identification/callsign.
    /// </summary>
    public string? Callsign { get; set; }

    /// <summary>
    /// Aircraft registration number (tail number).
    /// </summary>
    public string? Registration { get; set; }

    /// <summary>
    /// Indicates whether the aircraft is military.
    /// </summary>
    public bool IsMilitary { get; set; }

    /// <summary>
    /// Current latitude position in decimal degrees.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Current longitude position in decimal degrees.
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Current altitude in feet.
    /// </summary>
    public double? Altitude { get; set; }

    /// <summary>
    /// Current ground speed in knots.
    /// </summary>
    public double? Speed { get; set; }

    /// <summary>
    /// Current heading in degrees (0-359).
    /// </summary>
    public double? Heading { get; set; }

    /// <summary>
    /// Timestamp of the last update.
    /// </summary>
    public DateTime LastUpdate { get; set; }
}
