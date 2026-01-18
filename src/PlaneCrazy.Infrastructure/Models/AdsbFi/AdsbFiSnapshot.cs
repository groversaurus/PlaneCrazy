using System.Text.Json.Serialization;

namespace PlaneCrazy.Infrastructure.Models.AdsbFi;

/// <summary>
/// Represents a snapshot response from the adsb.fi API.
/// This is the top-level response structure containing an array of aircraft.
/// </summary>
public class AdsbFiSnapshot
{
    /// <summary>
    /// Array of aircraft in the response.
    /// </summary>
    [JsonPropertyName("aircraft")]
    public AdsbFiAircraft[]? Aircraft { get; set; }

    /// <summary>
    /// Current timestamp when the snapshot was generated.
    /// </summary>
    [JsonPropertyName("now")]
    public double? Now { get; set; }

    /// <summary>
    /// Total number of messages processed.
    /// </summary>
    [JsonPropertyName("messages")]
    public int? Messages { get; set; }

    /// <summary>
    /// Total number of aircraft in the response.
    /// </summary>
    [JsonPropertyName("total")]
    public int? Total { get; set; }
}
