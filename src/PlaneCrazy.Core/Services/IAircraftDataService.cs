using PlaneCrazy.Core.Models;

namespace PlaneCrazy.Core.Services;

/// <summary>
/// Service interface for managing aircraft data
/// </summary>
public interface IAircraftDataService
{
    /// <summary>
    /// Gets all tracked aircraft
    /// </summary>
    Task<IEnumerable<AircraftData>> GetAllAircraftAsync();

    /// <summary>
    /// Gets aircraft by ICAO address
    /// </summary>
    Task<AircraftData?> GetAircraftByIcaoAsync(string icao24);

    /// <summary>
    /// Adds or updates aircraft data
    /// </summary>
    Task AddOrUpdateAircraftAsync(AircraftData aircraft);

    /// <summary>
    /// Removes aircraft from tracking
    /// </summary>
    Task RemoveAircraftAsync(string icao24);
}
