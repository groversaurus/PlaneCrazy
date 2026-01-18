namespace PlaneCrazy.Domain.Queries.QueryResults;

/// <summary>
/// Result for aircraft near favourite airports query.
/// </summary>
public class AirportNearbyAircraftQueryResult
{
    public required string AirportIcaoCode { get; init; }
    public required string AirportName { get; init; }
    public double? AirportLatitude { get; init; }
    public double? AirportLongitude { get; init; }
    public List<AircraftDistanceQueryResult> NearbyAircraft { get; init; } = new();
}

public class AircraftDistanceQueryResult
{
    public required AircraftQueryResult Aircraft { get; init; }
    public double DistanceNauticalMiles { get; init; }
}
