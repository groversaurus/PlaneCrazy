namespace PlaneCrazy.Domain.Events;

public class TypeFavourited : DomainEvent
{
    public required string TypeCode { get; init; }
    public string? TypeName { get; init; }
}
