using PlaneCrazy.Core.Models;

namespace PlaneCrazy.Core.Repositories;

/// <summary>
/// Repository interface for ADS-B aircraft data operations.
/// </summary>
public interface IAdsBRepository
{
    /// <summary>
    /// Fetches an aircraft by its ICAO24 hex code.
    /// </summary>
    /// <param name="hex">The ICAO24 hex code (e.g., "A12345").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The aircraft if found, otherwise null.</returns>
    Task<Aircraft?> GetAircraftByHexAsync(string hex, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches aircraft by callsign.
    /// </summary>
    /// <param name="callsign">The flight callsign (e.g., "UAL123").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of aircraft matching the callsign.</returns>
    Task<IEnumerable<Aircraft>> GetAircraftByCallsignAsync(string callsign, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches an aircraft by its registration number.
    /// </summary>
    /// <param name="registration">The aircraft registration/tail number (e.g., "N12345").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The aircraft if found, otherwise null.</returns>
    Task<Aircraft?> GetAircraftByRegistrationAsync(string registration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches aircraft within a specified radius of a geographic location.
    /// </summary>
    /// <param name="latitude">Center point latitude in decimal degrees.</param>
    /// <param name="longitude">Center point longitude in decimal degrees.</param>
    /// <param name="radiusKilometers">Search radius in kilometers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of aircraft within the specified radius.</returns>
    Task<IEnumerable<Aircraft>> GetAircraftInRadiusAsync(
        double latitude, 
        double longitude, 
        double radiusKilometers, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches all military aircraft.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of military aircraft.</returns>
    Task<IEnumerable<Aircraft>> GetMilitaryAircraftAsync(CancellationToken cancellationToken = default);
}
