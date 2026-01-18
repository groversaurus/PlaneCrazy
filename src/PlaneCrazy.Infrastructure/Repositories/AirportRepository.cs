using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Infrastructure.Repositories;

public class AirportRepository : JsonFileRepository<Airport>
{
    public AirportRepository() 
        : base("airports.json")
    {
    }

    protected override string GetEntityId(Airport entity) => entity.IcaoCode;
}
