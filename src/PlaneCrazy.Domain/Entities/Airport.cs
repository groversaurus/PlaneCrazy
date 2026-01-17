namespace PlaneCrazy.Domain.Entities;

public class Airport
{
    public required string IcaoCode { get; init; }
    public string? IataCode { get; set; }
    public string? Name { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
