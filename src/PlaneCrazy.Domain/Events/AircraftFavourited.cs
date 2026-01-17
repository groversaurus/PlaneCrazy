namespace PlaneCrazy.Domain.Events;

public class AircraftFavourited : DomainEvent
{
    public required string Icao24 { get; init; }
    public string? Registration { get; init; }
    public string? TypeCode { get; init; }
}
