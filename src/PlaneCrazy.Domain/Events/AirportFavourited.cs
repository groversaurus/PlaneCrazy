namespace PlaneCrazy.Domain.Events;

public class AirportFavourited : DomainEvent
{
    public required string IcaoCode { get; init; }
    public string? Name { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
}
