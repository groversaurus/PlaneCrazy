using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Infrastructure.Repositories;

public class AircraftRepository : JsonFileRepository<Aircraft>
{
    public AircraftRepository(string basePath) 
        : base(basePath, "aircraft.json")
    {
    }

    protected override string GetEntityId(Aircraft entity) => entity.Icao24;
}
