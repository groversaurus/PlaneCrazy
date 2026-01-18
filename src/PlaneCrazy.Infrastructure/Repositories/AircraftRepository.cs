using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Infrastructure.Repositories;

public class AircraftRepository : JsonFileRepository<Aircraft>
{
    public AircraftRepository() 
        : base("aircraft.json")
    {
    }

    protected override string GetEntityId(Aircraft entity) => entity.Icao24;
}
