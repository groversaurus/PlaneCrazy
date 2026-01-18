namespace PlaneCrazy.Domain.Entities;

/// <summary>
/// Represents a user's favourite individual aircraft.
/// </summary>
public class AircraftFavourite
{
    /// <summary>
    /// The ICAO24 hex identifier of the favourited aircraft (e.g., "A12345").
    /// </summary>
    public required string Icao24 { get; init; }
    
    /// <summary>
    /// The aircraft's registration/tail number (e.g., "N12345", "G-ABCD").
    /// </summary>
    public string? Registration { get; init; }
    
    /// <summary>
    /// The aircraft type code (e.g., "B738", "A320").
    /// </summary>
    public string? TypeCode { get; init; }
    
    /// <summary>
    /// When this aircraft was added to favourites.
    /// </summary>
    public DateTime FavouritedAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// The user who favourited this aircraft.
    /// </summary>
    public string? FavouritedBy { get; init; }
    
    /// <summary>
    /// Optional notes or reasons for favouriting this aircraft.
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Last time this aircraft was seen/tracked.
    /// </summary>
    public DateTime? LastSeen { get; set; }
    
    /// <summary>
    /// Number of times this aircraft has been spotted by the user.
    /// </summary>
    public int SpottingCount { get; set; }
}
