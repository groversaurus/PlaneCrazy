namespace PlaneCrazy.Domain.Events;

public class AirportUnfavourited : DomainEvent
{
    public required string IcaoCode { get; init; }
}
