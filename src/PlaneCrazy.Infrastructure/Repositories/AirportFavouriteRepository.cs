using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Infrastructure.Repositories;

public class AirportFavouriteRepository : JsonFileRepository<AirportFavourite>
{
    public AirportFavouriteRepository() 
        : base("airport-favourites.json")
    {
    }

    protected override string GetEntityId(AirportFavourite entity) => entity.IcaoCode;
}
