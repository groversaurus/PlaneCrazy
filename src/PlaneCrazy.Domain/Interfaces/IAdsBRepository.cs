using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Domain.Interfaces;

public interface IAdsBRepository
{
    /// <summary>
    /// Fetches an aircraft by its ICAO24 hex identifier.
    /// </summary>
    /// <param name="hex">The ICAO24 hex code (e.g., "A12345").</param>
    /// <returns>The aircraft with the specified hex code, or null if not found.</returns>
    Task<Aircraft?> GetByHexAsync(string hex);

    /// <summary>
    /// Fetches aircraft by callsign (flight number).
    /// </summary>
    /// <param name="callsign">The flight callsign (e.g., "UAL123").</param>
    /// <returns>A collection of aircraft with the specified callsign.</returns>
    Task<IEnumerable<Aircraft>> GetByCallsignAsync(string callsign);

    /// <summary>
    /// Fetches an aircraft by its registration (tail number).
    /// </summary>
    /// <param name="registration">The aircraft registration (e.g., "N12345").</param>
    /// <returns>The aircraft with the specified registration, or null if not found.</returns>
    Task<Aircraft?> GetByRegistrationAsync(string registration);

    /// <summary>
    /// Fetches aircraft within a specified radius from a geographic point.
    /// </summary>
    /// <param name="latitude">The center latitude in decimal degrees.</param>
    /// <param name="longitude">The center longitude in decimal degrees.</param>
    /// <param name="radiusNauticalMiles">The search radius in nautical miles.</param>
    /// <returns>A collection of aircraft within the specified radius.</returns>
    Task<IEnumerable<Aircraft>> GetByRadiusAsync(double latitude, double longitude, double radiusNauticalMiles);

    /// <summary>
    /// Fetches military aircraft.
    /// Military aircraft are typically identified by ICAO24 hex ranges allocated to military operators,
    /// specific registration patterns, or ADS-B category codes indicating military aircraft types.
    /// </summary>
    /// <returns>A collection of military aircraft.</returns>
    Task<IEnumerable<Aircraft>> GetMilitaryAircraftAsync();
}
