using System.Text.Json.Serialization;

namespace PlaneCrazy.Core.Models;

/// <summary>
/// Response from adsb.fi API containing aircraft data
/// </summary>
public class AircraftResponse
{
    /// <summary>
    /// Current timestamp (seconds since epoch)
    /// </summary>
    [JsonPropertyName("now")]
    public double Now { get; set; }

    /// <summary>
    /// Number of messages processed
    /// </summary>
    [JsonPropertyName("messages")]
    public long Messages { get; set; }

    /// <summary>
    /// List of aircraft
    /// </summary>
    [JsonPropertyName("aircraft")]
    public List<Aircraft> Aircraft { get; set; } = new();
}
