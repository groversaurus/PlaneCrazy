namespace PlaneCrazy.Domain.Events;

public class AircraftUnfavourited : DomainEvent
{
    public required string Icao24 { get; init; }
}
