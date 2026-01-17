namespace PlaneCrazy.Models;

/// <summary>
/// Represents a geographic position with latitude, longitude, and altitude.
/// </summary>
public class Position
{
    /// <summary>
    /// Latitude in decimal degrees. Range: -90 to 90.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude in decimal degrees. Range: -180 to 180.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Altitude in feet. Can be barometric or geometric altitude.
    /// </summary>
    public int? Altitude { get; set; }

    /// <summary>
    /// Ground altitude in feet (altitude when on ground).
    /// </summary>
    public int? GroundAltitude { get; set; }
}
