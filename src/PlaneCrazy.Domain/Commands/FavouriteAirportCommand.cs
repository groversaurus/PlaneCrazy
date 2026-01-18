using PlaneCrazy.Domain.Validation;

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
        var result = CommandValidator.ValidateFavouriteAirport(this);
        if (!result.IsValid)
            throw new ValidationException(result);
    }
}
