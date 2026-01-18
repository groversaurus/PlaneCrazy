using PlaneCrazy.Domain.Validation;

namespace PlaneCrazy.Domain.Commands;

/// <summary>
/// Command to add an aircraft type to favourites.
/// </summary>
public class FavouriteAircraftTypeCommand : Command
{
    /// <summary>
    /// The aircraft type code (e.g., "B738", "A320").
    /// </summary>
    public required string TypeCode { get; init; }
    
    /// <summary>
    /// The aircraft type name (e.g., "Boeing 737-800") (optional).
    /// </summary>
    public string? TypeName { get; init; }
    
    /// <summary>
    /// The user favouriting the type (optional, falls back to IssuedBy).
    /// </summary>
    public string? User { get; init; }
    
    public override void Validate()
    {
        var result = CommandValidator.ValidateFavouriteAircraftType(this);
        if (!result.IsValid)
            throw new ValidationException(result);
    }
}
