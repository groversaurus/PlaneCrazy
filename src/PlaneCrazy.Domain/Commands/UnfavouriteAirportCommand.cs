namespace PlaneCrazy.Domain.Commands;

/// <summary>
/// Command to remove an airport from favourites.
/// </summary>
public class UnfavouriteAirportCommand : Command
{
    /// <summary>
    /// The ICAO airport code.
    /// </summary>
    public required string IcaoCode { get; init; }
    
    /// <summary>
    /// The user unfavouriting the airport (optional, falls back to IssuedBy).
    /// </summary>
    public string? User { get; init; }
    
    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(IcaoCode))
            throw new ArgumentException("IcaoCode cannot be empty.", nameof(IcaoCode));
    }
}
