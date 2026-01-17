namespace PlaneCrazy.Domain.Events;

public class AirportFavourited : DomainEvent
{
    public required string IcaoCode { get; init; }
    public string? Name { get; init; }
}
