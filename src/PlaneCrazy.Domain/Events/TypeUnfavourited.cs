namespace PlaneCrazy.Domain.Events;

public class TypeUnfavourited : DomainEvent
{
    public required string TypeCode { get; init; }
}
