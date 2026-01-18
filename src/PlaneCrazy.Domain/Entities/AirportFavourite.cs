namespace PlaneCrazy.Domain.Entities;

/// <summary>
/// Represents a user's favourite airport.
/// </summary>
public class AirportFavourite
{
    /// <summary>
    /// The ICAO airport code (e.g., "KJFK", "EGLL", "YSSY").
    /// This is the primary identifier for the airport.
    /// </summary>
    public required string IcaoCode { get; init; }
    
    /// <summary>
    /// The IATA airport code (e.g., "JFK", "LHR", "SYD").
    /// </summary>
    public string? IataCode { get; init; }
    
    /// <summary>
    /// The full name of the airport (e.g., "John F. Kennedy International Airport").
    /// </summary>
    public string? Name { get; init; }
    
    /// <summary>
    /// The city where the airport is located (e.g., "New York", "London").
    /// </summary>
    public string? City { get; init; }
    
    /// <summary>
    /// The country where the airport is located (e.g., "United States", "United Kingdom").
    /// </summary>
    public string? Country { get; init; }
    
    /// <summary>
    /// Airport latitude in decimal degrees.
    /// </summary>
    public double? Latitude { get; init; }
    
    /// <summary>
    /// Airport longitude in decimal degrees.
    /// </summary>
    public double? Longitude { get; init; }
    
    /// <summary>
    /// When this airport was added to favourites.
    /// </summary>
    public DateTime FavouritedAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// The user who favourited this airport.
    /// </summary>
    public string? FavouritedBy { get; init; }
    
    /// <summary>
    /// Optional notes about the airport (e.g., "Great spotting location").
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Number of times the user has visited this airport.
    /// </summary>
    public int VisitCount { get; set; }
    
    /// <summary>
    /// The user's personal rating of this airport (1-5 stars).
    /// </summary>
    public int? Rating { get; set; }
    
    /// <summary>
    /// Tags for categorizing the airport (e.g., "Spotting", "Visited", "Wishlist").
    /// </summary>
    public List<string> Tags { get; init; } = new();
}
