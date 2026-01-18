using PlaneCrazy.Domain.Validation;

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
        var result = CommandValidator.ValidateFavouriteAircraft(this);
        if (!result.IsValid)
            throw new ValidationException(result);
    }
}
