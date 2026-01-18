namespace PlaneCrazy.Domain.Commands;

/// <summary>
/// Command to add an aircraft to favourites.
/// </summary>
public class FavouriteAircraftCommand : Command
{
    /// <summary>
    /// The ICAO24 hex identifier of the aircraft.
    /// </summary>
    public required string Icao24 { get; init; }
    
    /// <summary>
    /// The aircraft registration (optional).
    /// </summary>
    public string? Registration { get; init; }
    
    /// <summary>
    /// The aircraft type code (optional).
    /// </summary>
    public string? TypeCode { get; init; }
    
    /// <summary>
    /// The user favouriting the aircraft (optional, falls back to IssuedBy).
    /// </summary>
    public string? User { get; init; }
    
    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Icao24))
            throw new ArgumentException("Icao24 cannot be empty.", nameof(Icao24));
        
        // ICAO24 should be 6 hex characters
        if (Icao24.Length != 6)
            throw new ArgumentException("Icao24 must be 6 characters.", nameof(Icao24));
        
        if (!System.Text.RegularExpressions.Regex.IsMatch(Icao24, "^[A-Fa-f0-9]{6}$"))
            throw new ArgumentException("Icao24 must be valid hex characters.", nameof(Icao24));
    }
}
