using System.Text.Json.Serialization;

namespace PlaneCrazy.Core.Models;

/// <summary>
/// Represents an aircraft with ADS-B data from adsb.fi API
/// </summary>
public class Aircraft
{
    /// <summary>
    /// ICAO 24-bit aircraft address
    /// </summary>
    [JsonPropertyName("hex")]
    public string Hex { get; set; } = string.Empty;

    /// <summary>
    /// Flight number or callsign
    /// </summary>
    [JsonPropertyName("flight")]
    public string? Flight { get; set; }

    /// <summary>
    /// Registration number
    /// </summary>
    [JsonPropertyName("r")]
    public string? Registration { get; set; }

    /// <summary>
    /// Aircraft type
    /// </summary>
    [JsonPropertyName("t")]
    public string? Type { get; set; }

    /// <summary>
    /// Altitude in feet
    /// </summary>
    [JsonPropertyName("alt_baro")]
    public int? Altitude { get; set; }

    /// <summary>
    /// Ground speed in knots
    /// </summary>
    [JsonPropertyName("gs")]
    public double? GroundSpeed { get; set; }

    /// <summary>
    /// Track angle in degrees
    /// </summary>
    [JsonPropertyName("track")]
    public double? Track { get; set; }

    /// <summary>
    /// Latitude in degrees
    /// </summary>
    [JsonPropertyName("lat")]
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude in degrees
    /// </summary>
    [JsonPropertyName("lon")]
    public double? Longitude { get; set; }

    /// <summary>
    /// Vertical rate in feet per minute
    /// </summary>
    [JsonPropertyName("baro_rate")]
    public int? VerticalRate { get; set; }

    /// <summary>
    /// Squawk code
    /// </summary>
    [JsonPropertyName("squawk")]
    public string? Squawk { get; set; }

    /// <summary>
    /// Emergency status
    /// </summary>
    [JsonPropertyName("emergency")]
    public string? Emergency { get; set; }

    /// <summary>
    /// Category descriptor
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Navigation altitude in feet
    /// </summary>
    [JsonPropertyName("nav_altitude_mcp")]
    public int? NavAltitude { get; set; }

    /// <summary>
    /// Navigation heading in degrees
    /// </summary>
    [JsonPropertyName("nav_heading")]
    public double? NavHeading { get; set; }

    /// <summary>
    /// Time at which the position was last updated (seconds since epoch)
    /// </summary>
    [JsonPropertyName("seen_pos")]
    public double? SeenPosition { get; set; }

    /// <summary>
    /// Time at which any message was last received (seconds since epoch)
    /// </summary>
    [JsonPropertyName("seen")]
    public double? Seen { get; set; }
}
