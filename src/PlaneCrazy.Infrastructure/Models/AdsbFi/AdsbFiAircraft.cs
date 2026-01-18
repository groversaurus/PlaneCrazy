using System.Text.Json.Serialization;

namespace PlaneCrazy.Infrastructure.Models.AdsbFi;

/// <summary>
/// Represents an aircraft object from the adsb.fi API response.
/// Maps to the JSON structure returned by adsb.fi endpoints.
/// </summary>
public class AdsbFiAircraft
{
    /// <summary>
    /// ICAO24 hex identifier (e.g., "a12345").
    /// </summary>
    [JsonPropertyName("hex")]
    public string? Hex { get; set; }

    /// <summary>
    /// Aircraft registration (e.g., "N12345").
    /// </summary>
    [JsonPropertyName("r")]
    public string? R { get; set; }

    /// <summary>
    /// Aircraft type code (e.g., "B738").
    /// </summary>
    [JsonPropertyName("t")]
    public string? T { get; set; }

    /// <summary>
    /// Flight number or callsign.
    /// </summary>
    [JsonPropertyName("flight")]
    public string? Flight { get; set; }

    /// <summary>
    /// Latitude in decimal degrees.
    /// </summary>
    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    /// <summary>
    /// Longitude in decimal degrees.
    /// </summary>
    [JsonPropertyName("lon")]
    public double? Lon { get; set; }

    /// <summary>
    /// Barometric altitude in feet.
    /// </summary>
    [JsonPropertyName("alt_baro")]
    public double? Alt_Baro { get; set; }

    /// <summary>
    /// Geometric altitude in feet.
    /// </summary>
    [JsonPropertyName("alt_geom")]
    public double? Alt_Geom { get; set; }

    /// <summary>
    /// Ground speed in knots.
    /// </summary>
    [JsonPropertyName("gs")]
    public double? Gs { get; set; }

    /// <summary>
    /// True airspeed in knots.
    /// </summary>
    [JsonPropertyName("tas")]
    public double? Tas { get; set; }

    /// <summary>
    /// Indicated airspeed in knots.
    /// </summary>
    [JsonPropertyName("ias")]
    public double? Ias { get; set; }

    /// <summary>
    /// Mach number.
    /// </summary>
    [JsonPropertyName("mach")]
    public double? Mach { get; set; }

    /// <summary>
    /// Track/heading in degrees (0-359).
    /// </summary>
    [JsonPropertyName("track")]
    public double? Track { get; set; }

    /// <summary>
    /// Rate of turn in degrees per second.
    /// </summary>
    [JsonPropertyName("track_rate")]
    public double? Track_Rate { get; set; }

    /// <summary>
    /// Roll angle in degrees.
    /// </summary>
    [JsonPropertyName("roll")]
    public double? Roll { get; set; }

    /// <summary>
    /// Magnetic heading in degrees.
    /// </summary>
    [JsonPropertyName("mag_heading")]
    public double? Mag_Heading { get; set; }

    /// <summary>
    /// True heading in degrees.
    /// </summary>
    [JsonPropertyName("true_heading")]
    public double? True_Heading { get; set; }

    /// <summary>
    /// Barometric vertical rate in feet per minute.
    /// </summary>
    [JsonPropertyName("baro_rate")]
    public double? Baro_Rate { get; set; }

    /// <summary>
    /// Geometric vertical rate in feet per minute.
    /// </summary>
    [JsonPropertyName("geom_rate")]
    public double? Geom_Rate { get; set; }

    /// <summary>
    /// Transponder squawk code.
    /// </summary>
    [JsonPropertyName("squawk")]
    public string? Squawk { get; set; }

    /// <summary>
    /// Emergency status.
    /// </summary>
    [JsonPropertyName("emergency")]
    public string? Emergency { get; set; }

    /// <summary>
    /// Aircraft category.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Navigation QNH setting.
    /// </summary>
    [JsonPropertyName("nav_qnh")]
    public double? Nav_Qnh { get; set; }

    /// <summary>
    /// Autopilot selected altitude (MCP/FCU) in feet.
    /// </summary>
    [JsonPropertyName("nav_altitude_mcp")]
    public double? Nav_Altitude_Mcp { get; set; }

    /// <summary>
    /// FMS selected altitude in feet.
    /// </summary>
    [JsonPropertyName("nav_altitude_fms")]
    public double? Nav_Altitude_Fms { get; set; }

    /// <summary>
    /// Autopilot selected heading in degrees.
    /// </summary>
    [JsonPropertyName("nav_heading")]
    public double? Nav_Heading { get; set; }

    /// <summary>
    /// Navigation modes array.
    /// </summary>
    [JsonPropertyName("nav_modes")]
    public string[]? Nav_Modes { get; set; }

    /// <summary>
    /// Navigation Integrity Category.
    /// </summary>
    [JsonPropertyName("nic")]
    public int? Nic { get; set; }

    /// <summary>
    /// Radius of Containment in meters.
    /// </summary>
    [JsonPropertyName("rc")]
    public double? Rc { get; set; }

    /// <summary>
    /// Seconds since position was last seen.
    /// </summary>
    [JsonPropertyName("seen_pos")]
    public double? Seen_Pos { get; set; }

    /// <summary>
    /// Seconds since any message was last seen.
    /// </summary>
    [JsonPropertyName("seen")]
    public double? Seen { get; set; }

    /// <summary>
    /// Signal strength (RSSI).
    /// </summary>
    [JsonPropertyName("rssi")]
    public double? Rssi { get; set; }

    /// <summary>
    /// Message count.
    /// </summary>
    [JsonPropertyName("messages")]
    public int? Messages { get; set; }

    /// <summary>
    /// TIS-B sources array.
    /// </summary>
    [JsonPropertyName("tisb")]
    public string[]? Tisb { get; set; }

    /// <summary>
    /// MLAT sources array.
    /// </summary>
    [JsonPropertyName("mlat")]
    public string[]? Mlat { get; set; }

    /// <summary>
    /// ADS-B version.
    /// </summary>
    [JsonPropertyName("version")]
    public int? Version { get; set; }
}
