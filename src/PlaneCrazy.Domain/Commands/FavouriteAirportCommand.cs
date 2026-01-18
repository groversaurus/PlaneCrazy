namespace PlaneCrazy.Domain.Commands;

/// <summary>
/// Command to add an airport to favourites.
/// </summary>
public class FavouriteAirportCommand : Command
{
    /// <summary>
    /// The ICAO airport code (e.g., "KJFK", "EGLL").
    /// </summary>
    public required string IcaoCode { get; init; }
    
    /// <summary>
    /// The airport name (e.g., "John F. Kennedy International Airport") (optional).
    /// </summary>
    public string? Name { get; init; }
    
    /// <summary>
    /// The user favouriting the airport (optional, falls back to IssuedBy).
    /// </summary>
    public string? User { get; init; }
    
    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(IcaoCode))
            throw new ArgumentException("IcaoCode cannot be empty.", nameof(IcaoCode));
        
        // ICAO codes are 4 characters
        if (IcaoCode.Length != 4)
            throw new ArgumentException("IcaoCode must be 4 characters.", nameof(IcaoCode));
        
        if (!System.Text.RegularExpressions.Regex.IsMatch(IcaoCode, "^[A-Z]{4}$"))
            throw new ArgumentException("IcaoCode must be 4 uppercase letters.", nameof(IcaoCode));
    }
}
