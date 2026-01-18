using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.Repositories;

namespace PlaneCrazy.Infrastructure.Projections;

public class FavouriteProjection : IProjection
{
    private readonly IEventStore _eventStore;
    private readonly FavouriteRepository _favouriteRepository;

    public string ProjectionName => "FavouriteProjection";

    public FavouriteProjection(IEventStore eventStore, FavouriteRepository favouriteRepository)
    {
        _eventStore = eventStore;
        _favouriteRepository = favouriteRepository;
    }

    public async Task RebuildAsync()
    {
        var events = await _eventStore.GetAllAsync();
        
        foreach (var @event in events)
        {
            await ApplyEventAsync(@event);
        }
    }

    public async Task<bool> ApplyEventAsync(DomainEvent @event)
    {
        switch (@event)
        {
            case AircraftFavourited aircraftFavourited:
                await _favouriteRepository.SaveAsync(new Domain.Entities.Favourite
                {
                    EntityType = "Aircraft",
                    EntityId = aircraftFavourited.Icao24,
                    FavouritedAt = aircraftFavourited.OccurredAt,
                    Metadata = new Dictionary<string, string>
                    {
                        ["Registration"] = aircraftFavourited.Registration ?? "",
                        ["TypeCode"] = aircraftFavourited.TypeCode ?? ""
                    }
                });
                return true;

            case AircraftUnfavourited aircraftUnfavourited:
                await _favouriteRepository.DeleteAsync($"Aircraft_{aircraftUnfavourited.Icao24}");
                return true;

            case TypeFavourited typeFavourited:
                await _favouriteRepository.SaveAsync(new Domain.Entities.Favourite
                {
                    EntityType = "Type",
                    EntityId = typeFavourited.TypeCode,
                    FavouritedAt = typeFavourited.OccurredAt,
                    Metadata = new Dictionary<string, string>
                    {
                        ["TypeName"] = typeFavourited.TypeName ?? ""
                    }
                });
                return true;

            case TypeUnfavourited typeUnfavourited:
                await _favouriteRepository.DeleteAsync($"Type_{typeUnfavourited.TypeCode}");
                return true;

            case AirportFavourited airportFavourited:
                await _favouriteRepository.SaveAsync(new Domain.Entities.Favourite
                {
                    EntityType = "Airport",
                    EntityId = airportFavourited.IcaoCode,
                    FavouritedAt = airportFavourited.OccurredAt,
                    Metadata = new Dictionary<string, string>
                    {
                        ["Name"] = airportFavourited.Name ?? ""
                    }
                });
                return true;

            case AirportUnfavourited airportUnfavourited:
                await _favouriteRepository.DeleteAsync($"Airport_{airportUnfavourited.IcaoCode}");
                return true;
            default:
                return false;
        }
    }
}
