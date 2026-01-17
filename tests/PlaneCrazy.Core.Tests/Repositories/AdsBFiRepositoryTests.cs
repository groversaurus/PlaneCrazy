using FluentAssertions;
using Moq;
using PlaneCrazy.Core.Models;
using PlaneCrazy.Core.Repositories;
using PlaneCrazy.Core.Services;

namespace PlaneCrazy.Core.Tests.Repositories;

public class AdsBFiRepositoryTests
{
    private readonly Mock<ApiClient> _mockApiClient;
    private readonly AdsBFiRepository _repository;

    public AdsBFiRepositoryTests()
    {
        var mockHttpClient = new HttpClient();
        _mockApiClient = new Mock<ApiClient>(mockHttpClient);
        _repository = new AdsBFiRepository(_mockApiClient.Object);
    }

    [Fact]
    public void Constructor_WithNullApiClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new AdsBFiRepository(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("apiClient");
    }

    [Fact]
    public void Constructor_WithCustomBaseUrl_UsesCustomBaseUrl()
    {
        // Arrange
        var mockHttpClient = new HttpClient();
        var mockApiClient = new Mock<ApiClient>(mockHttpClient);
        var customBaseUrl = "https://custom.api.com";

        // Act
        var repository = new AdsBFiRepository(mockApiClient.Object, customBaseUrl);

        // Assert
        repository.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAircraftAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var expectedResponse = new AircraftResponse
        {
            Now = 1234567890,
            Messages = 100,
            Aircraft = new List<Aircraft>
            {
                new Aircraft { Hex = "ABC123", Flight = "TEST123" }
            }
        };

        _mockApiClient
            .Setup(x => x.GetAsync<AircraftResponse>(
                It.Is<string>(url => url.EndsWith("/all")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _repository.GetAllAircraftAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Aircraft.Should().HaveCount(1);
        result.Aircraft[0].Hex.Should().Be("ABC123");
        _mockApiClient.Verify(
            x => x.GetAsync<AircraftResponse>(
                It.Is<string>(url => url.EndsWith("/all")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAircraftByLocationAsync_WithValidParameters_CallsCorrectEndpoint()
    {
        // Arrange
        var latitude = 60.1699;
        var longitude = 24.9384;
        var radiusNm = 50.0;
        var expectedResponse = new AircraftResponse
        {
            Now = 1234567890,
            Messages = 50,
            Aircraft = new List<Aircraft>
            {
                new Aircraft { Hex = "ABC123", Latitude = 60.17, Longitude = 24.94 }
            }
        };

        _mockApiClient
            .Setup(x => x.GetAsync<AircraftResponse>(
                It.Is<string>(url => url.Contains($"/lat/{latitude}/lon/{longitude}/dist/{radiusNm}")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _repository.GetAircraftByLocationAsync(latitude, longitude, radiusNm);

        // Assert
        result.Should().NotBeNull();
        result!.Aircraft.Should().HaveCount(1);
        _mockApiClient.Verify(
            x => x.GetAsync<AircraftResponse>(
                It.Is<string>(url => url.Contains($"/lat/{latitude}/lon/{longitude}/dist/{radiusNm}")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(-91, 0, 50)]
    [InlineData(91, 0, 50)]
    public async Task GetAircraftByLocationAsync_WithInvalidLatitude_ThrowsArgumentOutOfRangeException(
        double latitude, double longitude, double radiusNm)
    {
        // Act & Assert
        var act = async () => await _repository.GetAircraftByLocationAsync(latitude, longitude, radiusNm);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("latitude");
    }

    [Theory]
    [InlineData(0, -181, 50)]
    [InlineData(0, 181, 50)]
    public async Task GetAircraftByLocationAsync_WithInvalidLongitude_ThrowsArgumentOutOfRangeException(
        double latitude, double longitude, double radiusNm)
    {
        // Act & Assert
        var act = async () => await _repository.GetAircraftByLocationAsync(latitude, longitude, radiusNm);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("longitude");
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 0, -1)]
    public async Task GetAircraftByLocationAsync_WithInvalidRadius_ThrowsArgumentOutOfRangeException(
        double latitude, double longitude, double radiusNm)
    {
        // Act & Assert
        var act = async () => await _repository.GetAircraftByLocationAsync(latitude, longitude, radiusNm);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("radiusNm");
    }

    [Fact]
    public async Task GetAircraftByHexAsync_WithValidHex_ReturnsAircraft()
    {
        // Arrange
        var hex = "ABC123";
        var expectedResponse = new AircraftResponse
        {
            Now = 1234567890,
            Messages = 1,
            Aircraft = new List<Aircraft>
            {
                new Aircraft { Hex = hex, Flight = "TEST123" }
            }
        };

        _mockApiClient
            .Setup(x => x.GetAsync<AircraftResponse>(
                It.Is<string>(url => url.EndsWith($"/hex/{hex}")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _repository.GetAircraftByHexAsync(hex);

        // Assert
        result.Should().NotBeNull();
        result!.Hex.Should().Be(hex);
        _mockApiClient.Verify(
            x => x.GetAsync<AircraftResponse>(
                It.Is<string>(url => url.EndsWith($"/hex/{hex}")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAircraftByHexAsync_WhenAircraftNotFound_ReturnsNull()
    {
        // Arrange
        var hex = "NOTFOUND";
        var expectedResponse = new AircraftResponse
        {
            Now = 1234567890,
            Messages = 0,
            Aircraft = new List<Aircraft>()
        };

        _mockApiClient
            .Setup(x => x.GetAsync<AircraftResponse>(
                It.Is<string>(url => url.EndsWith($"/hex/{hex}")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _repository.GetAircraftByHexAsync(hex);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetAircraftByHexAsync_WithInvalidHex_ThrowsArgumentException(string? hex)
    {
        // Act & Assert
        var act = async () => await _repository.GetAircraftByHexAsync(hex!);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("hex");
    }
}
