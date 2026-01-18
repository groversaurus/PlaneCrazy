using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Infrastructure.Repositories;

public class AircraftFavouriteRepository : JsonFileRepository<AircraftFavourite>
{
    public AircraftFavouriteRepository() 
        : base("aircraft-favourites.json")
    {
    }

    protected override string GetEntityId(AircraftFavourite entity) => entity.Icao24;
}
