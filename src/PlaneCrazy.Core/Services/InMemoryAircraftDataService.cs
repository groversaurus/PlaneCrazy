using System.Collections.Concurrent;
using PlaneCrazy.Core.Models;

namespace PlaneCrazy.Core.Services;

/// <summary>
/// In-memory implementation of aircraft data service
/// </summary>
public class InMemoryAircraftDataService : IAircraftDataService
{
    private readonly ConcurrentDictionary<string, AircraftData> _aircraft = new();

    public Task<IEnumerable<AircraftData>> GetAllAircraftAsync()
    {
        return Task.FromResult<IEnumerable<AircraftData>>(_aircraft.Values.ToList());
    }

    public Task<AircraftData?> GetAircraftByIcaoAsync(string icao24)
    {
        _aircraft.TryGetValue(icao24, out var aircraft);
        return Task.FromResult(aircraft);
    }

    public Task AddOrUpdateAircraftAsync(AircraftData aircraft)
    {
        _aircraft.AddOrUpdate(aircraft.Icao24, aircraft, (_, _) => aircraft);
        return Task.CompletedTask;
    }

    public Task RemoveAircraftAsync(string icao24)
    {
        _aircraft.TryRemove(icao24, out _);
        return Task.CompletedTask;
    }
}
