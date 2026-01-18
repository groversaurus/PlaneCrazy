namespace PlaneCrazy.Domain.Queries.QueryResults;

/// <summary>
/// Read-optimized result for aircraft queries.
/// </summary>
public class AircraftQueryResult
{
    public required string Icao24 { get; init; }
    public string? Registration { get; init; }
    public string? TypeCode { get; init; }
    public string? Callsign { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? Altitude { get; init; }
    public double? Velocity { get; init; }
    public double? Track { get; init; }
    public bool OnGround { get; init; }
    public DateTime LastSeen { get; init; }
    public DateTime LastUpdated { get; init; }
    public int TotalUpdates { get; init; }
    
    // Enriched data
    public bool IsFavourited { get; init; }
    public int CommentCount { get; init; }
    public string? Origin { get; init; }
    public string? Destination { get; init; }
}
