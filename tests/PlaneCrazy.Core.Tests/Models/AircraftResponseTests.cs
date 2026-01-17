using FluentAssertions;
using PlaneCrazy.Core.Models;
using System.Text.Json;

namespace PlaneCrazy.Core.Tests.Models;

public class AircraftResponseTests
{
    [Fact]
    public void AircraftResponse_CanBeCreated()
    {
        // Act
        var response = new AircraftResponse
        {
            Now = 1234567890,
            Messages = 100,
            Aircraft = new List<Aircraft>
            {
                new Aircraft { Hex = "ABC123" },
                new Aircraft { Hex = "DEF456" }
            }
        };

        // Assert
        response.Now.Should().Be(1234567890);
        response.Messages.Should().Be(100);
        response.Aircraft.Should().HaveCount(2);
    }

    [Fact]
    public void AircraftResponse_DeserializesFromApiJson()
    {
        // Arrange
        var json = @"{
            ""now"": 1234567890.5,
            ""messages"": 1000,
            ""aircraft"": [
                {
                    ""hex"": ""ABC123"",
                    ""flight"": ""TEST123"",
                    ""lat"": 60.1699,
                    ""lon"": 24.9384
                },
                {
                    ""hex"": ""DEF456"",
                    ""flight"": ""TEST456"",
                    ""lat"": 61.1699,
                    ""lon"": 25.9384
                }
            ]
        }";

        // Act
        var response = JsonSerializer.Deserialize<AircraftResponse>(json);

        // Assert
        response.Should().NotBeNull();
        response!.Now.Should().Be(1234567890.5);
        response.Messages.Should().Be(1000);
        response.Aircraft.Should().HaveCount(2);
        response.Aircraft[0].Hex.Should().Be("ABC123");
        response.Aircraft[1].Hex.Should().Be("DEF456");
    }

    [Fact]
    public void AircraftResponse_SerializesCorrectly()
    {
        // Arrange
        var response = new AircraftResponse
        {
            Now = 1234567890,
            Messages = 100,
            Aircraft = new List<Aircraft>
            {
                new Aircraft { Hex = "ABC123", Flight = "TEST123" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<AircraftResponse>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Now.Should().Be(response.Now);
        deserialized.Messages.Should().Be(response.Messages);
        deserialized.Aircraft.Should().HaveCount(1);
        deserialized.Aircraft[0].Hex.Should().Be("ABC123");
    }
}
