using PlaneCrazy.Domain.Queries.QueryResults;

namespace PlaneCrazy.Domain.Interfaces;

/// <summary>
/// Query service for aircraft read operations.
/// Uses projections only - no event store access.
/// </summary>
public interface IAircraftQueryService
{
    /// <summary>
    /// Gets an aircraft by its ICAO24 hex identifier.
    /// </summary>
    Task<AircraftQueryResult?> GetByHexAsync(string icao24);
    
    /// <summary>
    /// Gets all aircraft currently tracked.
    /// </summary>
    Task<IEnumerable<AircraftQueryResult>> GetAllAircraftAsync();
    
    /// <summary>
    /// Gets aircraft with comment count and favourite status enriched.
    /// </summary>
    Task<IEnumerable<AircraftQueryResult>> GetAircraftWithEnrichedDataAsync();
    
    /// <summary>
    /// Gets detailed aircraft information including all comments.
    /// </summary>
    Task<AircraftWithDetailsQueryResult?> GetAircraftWithDetailsAsync(string icao24);
    
    /// <summary>
    /// Gets all aircraft that have comments.
    /// </summary>
    Task<IEnumerable<AircraftQueryResult>> GetAircraftWithCommentsAsync();
    
    /// <summary>
    /// Gets aircraft near favourite airports within a specified radius.
    /// </summary>
    /// <param name="radiusNauticalMiles">Search radius in nautical miles.</param>
    Task<IEnumerable<AirportNearbyAircraftQueryResult>> GetAircraftNearFavouriteAirportsAsync(double radiusNauticalMiles = 50);
    
    /// <summary>
    /// Gets aircraft by callsign.
    /// </summary>
    Task<IEnumerable<AircraftQueryResult>> GetByCallsignAsync(string callsign);
    
    /// <summary>
    /// Gets aircraft by type code.
    /// </summary>
    Task<IEnumerable<AircraftQueryResult>> GetByTypeCodeAsync(string typeCode);
}
