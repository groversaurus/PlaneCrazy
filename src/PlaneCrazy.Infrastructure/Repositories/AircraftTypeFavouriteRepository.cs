using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Infrastructure.Repositories;

public class AircraftTypeFavouriteRepository : JsonFileRepository<AircraftTypeFavourite>
{
    public AircraftTypeFavouriteRepository() 
        : base("aircraft-type-favourites.json")
    {
    }

    protected override string GetEntityId(AircraftTypeFavourite entity) => entity.TypeCode;
}
