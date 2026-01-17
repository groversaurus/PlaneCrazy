using FluentAssertions;
using PlaneCrazy.Core.Models;
using System.Text.Json;

namespace PlaneCrazy.Core.Tests.Models;

public class AircraftTests
{
    [Fact]
    public void Aircraft_CanBeCreated()
    {
        // Act
        var aircraft = new Aircraft
        {
            Hex = "ABC123",
            Flight = "TEST123",
            Latitude = 60.1699,
            Longitude = 24.9384,
            Altitude = 35000,
            GroundSpeed = 450.5,
            Track = 180.0
        };

        // Assert
        aircraft.Hex.Should().Be("ABC123");
        aircraft.Flight.Should().Be("TEST123");
        aircraft.Latitude.Should().Be(60.1699);
        aircraft.Longitude.Should().Be(24.9384);
        aircraft.Altitude.Should().Be(35000);
        aircraft.GroundSpeed.Should().Be(450.5);
        aircraft.Track.Should().Be(180.0);
    }

    [Fact]
    public void Aircraft_SerializesCorrectly()
    {
        // Arrange
        var aircraft = new Aircraft
        {
            Hex = "ABC123",
            Flight = "TEST123",
            Latitude = 60.1699,
            Longitude = 24.9384
        };

        // Act
        var json = JsonSerializer.Serialize(aircraft);
        var deserialized = JsonSerializer.Deserialize<Aircraft>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Hex.Should().Be(aircraft.Hex);
        deserialized.Flight.Should().Be(aircraft.Flight);
        deserialized.Latitude.Should().Be(aircraft.Latitude);
        deserialized.Longitude.Should().Be(aircraft.Longitude);
    }

    [Fact]
    public void Aircraft_DeserializesFromApiJson()
    {
        // Arrange
        var json = @"{
            ""hex"": ""ABC123"",
            ""flight"": ""TEST123"",
            ""r"": ""N123AB"",
            ""t"": ""B738"",
            ""alt_baro"": 35000,
            ""gs"": 450.5,
            ""track"": 180.0,
            ""lat"": 60.1699,
            ""lon"": 24.9384,
            ""baro_rate"": 0,
            ""squawk"": ""1200"",
            ""category"": ""A3"",
            ""seen"": 1.5,
            ""seen_pos"": 2.1
        }";

        // Act
        var aircraft = JsonSerializer.Deserialize<Aircraft>(json);

        // Assert
        aircraft.Should().NotBeNull();
        aircraft!.Hex.Should().Be("ABC123");
        aircraft.Flight.Should().Be("TEST123");
        aircraft.Registration.Should().Be("N123AB");
        aircraft.Type.Should().Be("B738");
        aircraft.Altitude.Should().Be(35000);
        aircraft.GroundSpeed.Should().Be(450.5);
        aircraft.Track.Should().Be(180.0);
        aircraft.Latitude.Should().Be(60.1699);
        aircraft.Longitude.Should().Be(24.9384);
        aircraft.Squawk.Should().Be("1200");
    }
}
