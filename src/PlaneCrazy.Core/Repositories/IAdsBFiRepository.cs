using PlaneCrazy.Core.Models;

namespace PlaneCrazy.Core.Repositories;

/// <summary>
/// Repository for accessing adsb.fi API endpoints
/// </summary>
public interface IAdsBFiRepository
{
    /// <summary>
    /// Gets all aircraft currently being tracked
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aircraft response containing all tracked aircraft</returns>
    Task<AircraftResponse?> GetAllAircraftAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets aircraft within a specific geographic area
    /// </summary>
    /// <param name="latitude">Center latitude</param>
    /// <param name="longitude">Center longitude</param>
    /// <param name="radiusNm">Radius in nautical miles</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aircraft response containing aircraft in the specified area</returns>
    Task<AircraftResponse?> GetAircraftByLocationAsync(
        double latitude, 
        double longitude, 
        double radiusNm, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific aircraft by its ICAO hex code
    /// </summary>
    /// <param name="hex">ICAO 24-bit aircraft address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aircraft with the specified hex code, or null if not found</returns>
    Task<Aircraft?> GetAircraftByHexAsync(string hex, CancellationToken cancellationToken = default);
}
