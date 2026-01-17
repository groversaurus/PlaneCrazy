namespace PlaneCrazy.Models.Tests;

public class SnapshotTests
{
    [Fact]
    public void Snapshot_CanBeCreated()
    {
        // Arrange & Act
        var snapshot = new Snapshot
        {
            Timestamp = DateTime.UtcNow,
            Aircraft = new Aircraft
            {
                Hex = "a1b2c3",
                Flight = "DAL456"
            },
            Position = new Position
            {
                Latitude = 40.7128,
                Longitude = -74.0060,
                Altitude = 35000
            },
            Seen = 1609459200,
            SeenPos = 1609459200,
            Messages = 150,
            Rssi = -15.5
        };

        // Assert
        Assert.NotNull(snapshot.Aircraft);
        Assert.NotNull(snapshot.Position);
        Assert.Equal("a1b2c3", snapshot.Aircraft.Hex);
        Assert.Equal(40.7128, snapshot.Position.Latitude);
        Assert.Equal(150, snapshot.Messages);
    }

    [Fact]
    public void Snapshot_InitializesAircraftAndPosition()
    {
        // Arrange & Act
        var snapshot = new Snapshot();

        // Assert
        Assert.NotNull(snapshot.Aircraft);
        Assert.NotNull(snapshot.Position);
    }

    [Fact]
    public void Snapshot_CanHaveNullableProperties()
    {
        // Arrange & Act
        var snapshot = new Snapshot
        {
            Timestamp = DateTime.UtcNow,
            Seen = null,
            SeenPos = null,
            Messages = null,
            Rssi = null
        };

        // Assert
        Assert.Null(snapshot.Seen);
        Assert.Null(snapshot.SeenPos);
        Assert.Null(snapshot.Messages);
        Assert.Null(snapshot.Rssi);
    }
}
