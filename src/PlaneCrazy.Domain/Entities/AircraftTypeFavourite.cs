namespace PlaneCrazy.Domain.Entities;

/// <summary>
/// Represents a user's favourite aircraft type/model.
/// </summary>
public class AircraftTypeFavourite
{
    /// <summary>
    /// The aircraft type code (e.g., "B738", "A320", "B77W").
    /// This is the primary identifier for the type.
    /// </summary>
    public required string TypeCode { get; init; }
    
    /// <summary>
    /// The common name of the aircraft type (e.g., "Boeing 737-800", "Airbus A320").
    /// </summary>
    public string? TypeName { get; init; }
    
    /// <summary>
    /// The manufacturer (e.g., "Boeing", "Airbus", "Embraer").
    /// </summary>
    public string? Manufacturer { get; init; }
    
    /// <summary>
    /// The specific model designation (e.g., "737-800", "A320-200").
    /// </summary>
    public string? Model { get; init; }
    
    /// <summary>
    /// When this aircraft type was added to favourites.
    /// </summary>
    public DateTime FavouritedAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// The user who favourited this type.
    /// </summary>
    public string? FavouritedBy { get; init; }
    
    /// <summary>
    /// Optional notes about why this type is a favourite.
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Count of aircraft of this type that have been spotted.
    /// </summary>
    public int SpottedCount { get; set; }
    
    /// <summary>
    /// The user's personal rating of this aircraft type (1-5 stars).
    /// </summary>
    public int? Rating { get; set; }
}
