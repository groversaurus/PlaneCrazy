using PlaneCrazy.Core.Models;
using PlaneCrazy.Core.Services;

namespace PlaneCrazy.Core.Tests.Services;

public class InMemoryAircraftDataServiceTests
{
    private readonly InMemoryAircraftDataService _service;

    public InMemoryAircraftDataServiceTests()
    {
        _service = new InMemoryAircraftDataService();
    }

    [Fact]
    public async Task AddOrUpdateAircraftAsync_AddsNewAircraft()
    {
        // Arrange
        var aircraft = new AircraftData
        {
            Icao24 = "TEST01",
            Callsign = "TST001"
        };

        // Act
        await _service.AddOrUpdateAircraftAsync(aircraft);
        var result = await _service.GetAircraftByIcaoAsync("TEST01");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEST01", result.Icao24);
        Assert.Equal("TST001", result.Callsign);
    }

    [Fact]
    public async Task AddOrUpdateAircraftAsync_UpdatesExistingAircraft()
    {
        // Arrange
        var aircraft1 = new AircraftData
        {
            Icao24 = "TEST01",
            Callsign = "TST001",
            Altitude = 10000
        };
        var aircraft2 = new AircraftData
        {
            Icao24 = "TEST01",
            Callsign = "TST002",
            Altitude = 20000
        };

        // Act
        await _service.AddOrUpdateAircraftAsync(aircraft1);
        await _service.AddOrUpdateAircraftAsync(aircraft2);
        var result = await _service.GetAircraftByIcaoAsync("TEST01");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TST002", result.Callsign);
        Assert.Equal(20000, result.Altitude);
    }

    [Fact]
    public async Task GetAircraftByIcaoAsync_ReturnsNullForNonExistent()
    {
        // Act
        var result = await _service.GetAircraftByIcaoAsync("NONEXISTENT");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAircraftAsync_ReturnsEmptyListWhenNone()
    {
        // Act
        var result = await _service.GetAllAircraftAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAircraftAsync_ReturnsAllAircraft()
    {
        // Arrange
        var aircraft1 = new AircraftData { Icao24 = "TEST01", Callsign = "TST001" };
        var aircraft2 = new AircraftData { Icao24 = "TEST02", Callsign = "TST002" };
        var aircraft3 = new AircraftData { Icao24 = "TEST03", Callsign = "TST003" };

        // Act
        await _service.AddOrUpdateAircraftAsync(aircraft1);
        await _service.AddOrUpdateAircraftAsync(aircraft2);
        await _service.AddOrUpdateAircraftAsync(aircraft3);
        var result = await _service.GetAllAircraftAsync();

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task RemoveAircraftAsync_RemovesAircraft()
    {
        // Arrange
        var aircraft = new AircraftData { Icao24 = "TEST01", Callsign = "TST001" };
        await _service.AddOrUpdateAircraftAsync(aircraft);

        // Act
        await _service.RemoveAircraftAsync("TEST01");
        var result = await _service.GetAircraftByIcaoAsync("TEST01");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAircraftAsync_DoesNotThrowForNonExistent()
    {
        // Act & Assert
        await _service.RemoveAircraftAsync("NONEXISTENT");
    }
}
