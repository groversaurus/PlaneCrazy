using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Domain.Queries.QueryResults;
using PlaneCrazy.Infrastructure.Repositories;

namespace PlaneCrazy.Infrastructure.QueryServices;

/// <summary>
/// Query service for favourite read operations using projections only.
/// </summary>
public class FavouriteQueryService : IFavouriteQueryService
{
    private readonly FavouriteRepository _favouriteRepository;
    private readonly CommentRepository _commentRepository;

    public FavouriteQueryService(
        FavouriteRepository favouriteRepository,
        CommentRepository commentRepository)
    {
        _favouriteRepository = favouriteRepository;
        _commentRepository = commentRepository;
    }

    public async Task<IEnumerable<FavouriteQueryResult>> GetAllFavouritesAsync()
    {
        var favourites = await _favouriteRepository.GetAllAsync();
        var results = new List<FavouriteQueryResult>();

        foreach (var fav in favourites)
        {
            results.Add(await MapToQueryResultAsync(fav));
        }

        return results;
    }

    public async Task<IEnumerable<FavouriteQueryResult>> GetFavouritesByTypeAsync(string entityType)
    {
        var favourites = await _favouriteRepository.GetByEntityTypeAsync(entityType);
        var results = new List<FavouriteQueryResult>();

        foreach (var fav in favourites)
        {
            results.Add(await MapToQueryResultAsync(fav));
        }

        return results;
    }

    public async Task<bool> IsFavouritedAsync(string entityType, string entityId)
    {
        var favourite = await _favouriteRepository.GetByIdAsync($"{entityType}_{entityId}");
        return favourite != null;
    }

    public async Task<FavouriteQueryResult?> GetFavouriteAsync(string entityType, string entityId)
    {
        var favourite = await _favouriteRepository.GetByIdAsync($"{entityType}_{entityId}");
        if (favourite == null) return null;

        return await MapToQueryResultAsync(favourite);
    }

    public async Task<IEnumerable<FavouriteQueryResult>> GetFavouriteAirportsAsync()
    {
        return await GetFavouritesByTypeAsync("Airport");
    }

    public async Task<IEnumerable<FavouriteQueryResult>> GetFavouriteAircraftAsync()
    {
        return await GetFavouritesByTypeAsync("Aircraft");
    }

    public async Task<IEnumerable<FavouriteQueryResult>> GetFavouriteTypesAsync()
    {
        return await GetFavouritesByTypeAsync("Type");
    }

    private async Task<FavouriteQueryResult> MapToQueryResultAsync(Domain.Entities.Favourite favourite)
    {
        var commentCount = await _commentRepository.GetActiveByEntityAsync(favourite.EntityType, favourite.EntityId);

        return new FavouriteQueryResult
        {
            EntityType = favourite.EntityType,
            EntityId = favourite.EntityId,
            FavouritedAt = favourite.FavouritedAt,
            Metadata = favourite.Metadata,
            CommentCount = commentCount.Count(),
            Registration = favourite.Metadata.GetValueOrDefault("Registration"),
            TypeCode = favourite.Metadata.GetValueOrDefault("TypeCode"),
            TypeName = favourite.Metadata.GetValueOrDefault("TypeName"),
            Name = favourite.Metadata.GetValueOrDefault("Name")
        };
    }
}
