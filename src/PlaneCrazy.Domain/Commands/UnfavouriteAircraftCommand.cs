namespace PlaneCrazy.Domain.Commands;

/// <summary>
/// Command to remove an aircraft from favourites.
/// </summary>
public class UnfavouriteAircraftCommand : Command
{
    /// <summary>
    /// The ICAO24 hex identifier of the aircraft.
    /// </summary>
    public required string Icao24 { get; init; }
    
    /// <summary>
    /// The user unfavouriting the aircraft (optional, falls back to IssuedBy).
    /// </summary>
    public string? User { get; init; }
    
    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Icao24))
            throw new ArgumentException("Icao24 cannot be empty.", nameof(Icao24));
    }
}
