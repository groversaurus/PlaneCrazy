namespace PlaneCrazy.Domain.Entities;

public class AircraftType
{
    public required string TypeCode { get; init; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Description { get; set; }
}
