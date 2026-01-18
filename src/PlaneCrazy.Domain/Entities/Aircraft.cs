namespace PlaneCrazy.Domain.Entities;

public class Aircraft
{
    // Identity
    public required string Icao24 { get; init; }
    public string? Registration { get; set; }
    public string? TypeCode { get; set; }
    public string? Callsign { get; set; }
    
    // Position
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Altitude { get; set; }
    
    // Movement
    public double? Velocity { get; set; }
    public double? Track { get; set; }
    public double? VerticalRate { get; set; }
    public bool OnGround { get; set; }
    
    // Flight Information
    public string? Squawk { get; set; }
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    
    // Timestamps
    public DateTime? FirstSeen { get; set; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Statistics
    public int TotalUpdates { get; set; }
}
