namespace PlaneCrazy.Domain.Interfaces;

/// <summary>
/// Service for looking up airport information by ICAO code.
/// </summary>
public interface IAirportLookupService
{
    /// <summary>
    /// Looks up an airport by its ICAO code.
    /// </summary>
    /// <param name="icaoCode">The 4-letter ICAO code (e.g., KJFK, EGLL).</param>
    /// <returns>Airport information if found, null otherwise.</returns>
    Task<AirportInfo?> LookupAsync(string icaoCode);
    
    /// <summary>
    /// Gets all known airports.
    /// </summary>
    /// <returns>Collection of all airports in the database.</returns>
    Task<IEnumerable<AirportInfo>> GetAllAsync();
}

/// <summary>
/// Airport information.
/// </summary>
public class AirportInfo
{
    public required string IcaoCode { get; set; }
    public required string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}
