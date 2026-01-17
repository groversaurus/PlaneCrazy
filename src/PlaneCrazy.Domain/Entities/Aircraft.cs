namespace PlaneCrazy.Domain.Entities;

public class Aircraft
{
    public required string Icao24 { get; init; }
    public string? Registration { get; set; }
    public string? TypeCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Altitude { get; set; }
    public double? Velocity { get; set; }
    public double? Track { get; set; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public bool OnGround { get; set; }
    public string? Callsign { get; set; }
    public string? Origin { get; set; }
    public string? Destination { get; set; }
}
