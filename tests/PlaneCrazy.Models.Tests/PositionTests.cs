namespace PlaneCrazy.Models.Tests;

public class PositionTests
{
    [Fact]
    public void Position_CanBeCreated()
    {
        // Arrange & Act
        var position = new Position
        {
            Latitude = 37.7749,
            Longitude = -122.4194,
            Altitude = 35000,
            GroundAltitude = 0
        };

        // Assert
        Assert.Equal(37.7749, position.Latitude);
        Assert.Equal(-122.4194, position.Longitude);
        Assert.Equal(35000, position.Altitude);
        Assert.Equal(0, position.GroundAltitude);
    }

    [Fact]
    public void Position_CanHaveNullAltitude()
    {
        // Arrange & Act
        var position = new Position
        {
            Latitude = 40.7128,
            Longitude = -74.0060,
            Altitude = null
        };

        // Assert
        Assert.Null(position.Altitude);
    }
}
