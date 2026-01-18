using PlaneCrazy.Domain.Queries.QueryResults;

namespace PlaneCrazy.Domain.Interfaces;

/// <summary>
/// Query service for favourite read operations.
/// Uses projections only - no event store access.
/// </summary>
public interface IFavouriteQueryService
{
    /// <summary>
    /// Gets all favourites.
    /// </summary>
    Task<IEnumerable<FavouriteQueryResult>> GetAllFavouritesAsync();
    
    /// <summary>
    /// Gets favourites by entity type (Aircraft, Type, Airport).
    /// </summary>
    Task<IEnumerable<FavouriteQueryResult>> GetFavouritesByTypeAsync(string entityType);
    
    /// <summary>
    /// Checks if a specific entity is favourited.
    /// </summary>
    Task<bool> IsFavouritedAsync(string entityType, string entityId);
    
    /// <summary>
    /// Gets a specific favourite with enriched data.
    /// </summary>
    Task<FavouriteQueryResult?> GetFavouriteAsync(string entityType, string entityId);
    
    /// <summary>
    /// Gets all favourite airports.
    /// </summary>
    Task<IEnumerable<FavouriteQueryResult>> GetFavouriteAirportsAsync();
    
    /// <summary>
    /// Gets all favourite aircraft.
    /// </summary>
    Task<IEnumerable<FavouriteQueryResult>> GetFavouriteAircraftAsync();
    
    /// <summary>
    /// Gets all favourite aircraft types.
    /// </summary>
    Task<IEnumerable<FavouriteQueryResult>> GetFavouriteTypesAsync();
}
