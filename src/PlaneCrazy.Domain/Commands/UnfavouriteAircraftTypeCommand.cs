using PlaneCrazy.Domain.Validation;

namespace PlaneCrazy.Domain.Commands;

/// <summary>
/// Command to remove an aircraft type from favourites.
/// </summary>
public class UnfavouriteAircraftTypeCommand : Command
{
    /// <summary>
    /// The aircraft type code.
    /// </summary>
    public required string TypeCode { get; init; }
    
    /// <summary>
    /// The user unfavouriting the type (optional, falls back to IssuedBy).
    /// </summary>
    public string? User { get; init; }
    
    public override void Validate()
    {
        var result = CommandValidator.ValidateUnfavouriteAircraftType(this);
        if (!result.IsValid)
            throw new ValidationException(result);
    }
}
