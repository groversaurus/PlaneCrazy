namespace PlaneCrazy.Models.Tests;

public class AircraftTests
{
    [Fact]
    public void Aircraft_CanBeCreated()
    {
        // Arrange & Act
        var aircraft = new Aircraft
        {
            Hex = "a1b2c3",
            Flight = "UAL123",
            Registration = "N12345",
            Type = "B738",
            Description = "Boeing 737-800",
            Category = EmitterCategory.Large,
            GroundSpeed = 450.5,
            Track = 270.0,
            VerticalRate = 1500,
            Squawk = "1200",
            OnGround = false,
            Emergency = false,
            Spi = false
        };

        // Assert
        Assert.Equal("a1b2c3", aircraft.Hex);
        Assert.Equal("UAL123", aircraft.Flight);
        Assert.Equal("N12345", aircraft.Registration);
        Assert.Equal("B738", aircraft.Type);
        Assert.Equal(EmitterCategory.Large, aircraft.Category);
        Assert.Equal(450.5, aircraft.GroundSpeed);
        Assert.False(aircraft.OnGround);
    }

    [Fact]
    public void Aircraft_CanHaveNullableProperties()
    {
        // Arrange & Act
        var aircraft = new Aircraft
        {
            Hex = "abc123"
        };

        // Assert
        Assert.Equal("abc123", aircraft.Hex);
        Assert.Null(aircraft.Flight);
        Assert.Null(aircraft.GroundSpeed);
        Assert.Null(aircraft.Category);
    }

    [Fact]
    public void Aircraft_CanIndicateEmergency()
    {
        // Arrange & Act
        var aircraft = new Aircraft
        {
            Hex = "emergency",
            Squawk = "7700",
            Emergency = true
        };

        // Assert
        Assert.True(aircraft.Emergency);
        Assert.Equal("7700", aircraft.Squawk);
    }
}
