using PlaneCrazy.Domain.Entities;

namespace PlaneCrazy.Domain.Interfaces;

public interface IAircraftDataService
{
    Task<IEnumerable<Aircraft>> FetchAircraftAsync();
}
