using PlaneCrazy.Core.Models;

namespace PlaneCrazy.Core.Tests.Models;

public class AircraftDataTests
{
    [Fact]
    public void AircraftData_Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var aircraft = new AircraftData();

        // Assert
        Assert.NotNull(aircraft.Icao24);
        Assert.Equal(string.Empty, aircraft.Icao24);
        Assert.True((DateTime.UtcNow - aircraft.LastSeen).TotalSeconds < 1);
    }

    [Fact]
    public void AircraftData_Properties_CanBeSet()
    {
        // Arrange
        var aircraft = new AircraftData();

        // Act
        aircraft.Icao24 = "A12345";
        aircraft.Callsign = "TEST123";
        aircraft.Latitude = 40.7128;
        aircraft.Longitude = -74.0060;
        aircraft.Altitude = 35000;
        aircraft.GroundSpeed = 450.5;
        aircraft.Heading = 90.0;
        aircraft.VerticalRate = 500;

        // Assert
        Assert.Equal("A12345", aircraft.Icao24);
        Assert.Equal("TEST123", aircraft.Callsign);
        Assert.Equal(40.7128, aircraft.Latitude);
        Assert.Equal(-74.0060, aircraft.Longitude);
        Assert.Equal(35000, aircraft.Altitude);
        Assert.Equal(450.5, aircraft.GroundSpeed);
        Assert.Equal(90.0, aircraft.Heading);
        Assert.Equal(500, aircraft.VerticalRate);
    }

    [Fact]
    public void AircraftData_NullableProperties_CanBeNull()
    {
        // Arrange & Act
        var aircraft = new AircraftData
        {
            Icao24 = "TEST01"
        };

        // Assert
        Assert.Null(aircraft.Callsign);
        Assert.Null(aircraft.Latitude);
        Assert.Null(aircraft.Longitude);
        Assert.Null(aircraft.Altitude);
        Assert.Null(aircraft.GroundSpeed);
        Assert.Null(aircraft.Heading);
        Assert.Null(aircraft.VerticalRate);
    }
}
