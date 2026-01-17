namespace PlaneCrazy.Models;

/// <summary>
/// Represents an aircraft with identification and flight information.
/// Based on adsb.fi JSON structure for ADS-B data.
/// </summary>
public class Aircraft
{
    /// <summary>
    /// ICAO 24-bit address (hex format), unique aircraft identifier.
    /// Example: "a1b2c3"
    /// </summary>
    public string? Hex { get; set; }

    /// <summary>
    /// Flight callsign or registration. Example: "UAL123"
    /// </summary>
    public string? Flight { get; set; }

    /// <summary>
    /// Aircraft registration (tail number). Example: "N12345"
    /// </summary>
    public string? Registration { get; set; }

    /// <summary>
    /// Aircraft type (ICAO aircraft type designator). Example: "B738" for Boeing 737-800.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Aircraft manufacturer and model description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Emitter category indicating the type of aircraft.
    /// </summary>
    public EmitterCategory? Category { get; set; }

    /// <summary>
    /// Ground speed in knots.
    /// </summary>
    public double? GroundSpeed { get; set; }

    /// <summary>
    /// True track (heading) in degrees (0-359).
    /// </summary>
    public double? Track { get; set; }

    /// <summary>
    /// Vertical rate in feet per minute. Positive is climbing, negative is descending.
    /// </summary>
    public int? VerticalRate { get; set; }

    /// <summary>
    /// Squawk code (transponder code). Example: "7700" for emergency.
    /// </summary>
    public string? Squawk { get; set; }

    /// <summary>
    /// Indicates if the aircraft is on the ground.
    /// </summary>
    public bool? OnGround { get; set; }

    /// <summary>
    /// Indicates if the aircraft is transmitting an emergency status.
    /// </summary>
    public bool? Emergency { get; set; }

    /// <summary>
    /// Special Position Identification pulse indicator.
    /// </summary>
    public bool? Spi { get; set; }
}
