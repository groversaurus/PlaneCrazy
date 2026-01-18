namespace PlaneCrazy.Infrastructure.Models.AdsbFi;

/// <summary>
/// Represents a geographic position with latitude and longitude coordinates.
/// </summary>
public class Position
{
    /// <summary>
    /// Latitude in decimal degrees. Positive values are North, negative values are South.
    /// </summary>
    public double? Lat { get; set; }

    /// <summary>
    /// Longitude in decimal degrees. Positive values are East, negative values are West.
    /// </summary>
    public double? Lon { get; set; }
}
