namespace PlaneCrazy.Domain.Models;

/// <summary>
/// Represents a complete snapshot of all aircraft at a specific point in time.
/// </summary>
public class AircraftSnapshot
{
    /// <summary>
    /// The timestamp this snapshot represents.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// All aircraft known at this time.
    /// </summary>
    public required List<Entities.Aircraft> Aircraft { get; init; }

    /// <summary>
    /// Total number of events processed to create this snapshot.
    /// </summary>
    public int EventCount { get; init; }

    /// <summary>
    /// Number of unique aircraft in this snapshot.
    /// </summary>
    public int AircraftCount { get; init; }

    /// <summary>
    /// Gets aircraft currently in the air (not on ground).
    /// </summary>
    public IEnumerable<Entities.Aircraft> AircraftInFlight =>
        Aircraft.Where(a => !a.OnGround && a.Altitude.HasValue && a.Altitude > 0);

    /// <summary>
    /// Gets aircraft on the ground.
    /// </summary>
    public IEnumerable<Entities.Aircraft> AircraftOnGround =>
        Aircraft.Where(a => a.OnGround);

    /// <summary>
    /// Gets aircraft with known positions.
    /// </summary>
    public IEnumerable<Entities.Aircraft> AircraftWithPosition =>
        Aircraft.Where(a => a.Latitude.HasValue && a.Longitude.HasValue);

    /// <summary>
    /// Gets aircraft grouped by type code.
    /// </summary>
    public Dictionary<string, int> AircraftByType =>
        Aircraft
            .Where(a => !string.IsNullOrEmpty(a.TypeCode))
            .GroupBy(a => a.TypeCode!)
            .ToDictionary(g => g.Key, g => g.Count());

    /// <summary>
    /// Gets the highest altitude aircraft.
    /// </summary>
    public Entities.Aircraft? HighestAircraft =>
        Aircraft
            .Where(a => a.Altitude.HasValue)
            .OrderByDescending(a => a.Altitude)
            .FirstOrDefault();

    /// <summary>
    /// Gets the fastest aircraft.
    /// </summary>
    public Entities.Aircraft? FastestAircraft =>
        Aircraft
            .Where(a => a.Velocity.HasValue)
            .OrderByDescending(a => a.Velocity)
            .FirstOrDefault();
}
