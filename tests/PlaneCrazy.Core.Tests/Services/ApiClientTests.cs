using FluentAssertions;
using Moq;
using Moq.Protected;
using PlaneCrazy.Core.Services;
using System.Net;
using System.Text.Json;

namespace PlaneCrazy.Core.Tests.Services;

public class ApiClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ApiClient _apiClient;

    public ApiClientTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _apiClient = new ApiClient(_httpClient);
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ApiClient(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public async Task GetAsync_WithValidUrl_ReturnsDeserializedObject()
    {
        // Arrange
        var testData = new TestDto { Name = "Test", Value = 123 };
        var jsonContent = JsonSerializer.Serialize(testData);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent)
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiClient.GetAsync<TestDto>("https://test.com/api");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(123);
    }

    private class TestDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public async Task GetAsync_WithNullUrl_ThrowsArgumentException()
    {
        // Act & Assert
        var act = async () => await _apiClient.GetAsync<object>(null!);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task GetAsync_WithEmptyUrl_ThrowsArgumentException()
    {
        // Act & Assert
        var act = async () => await _apiClient.GetAsync<object>("");
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task GetAsync_WithHttpError_ThrowsHttpRequestException()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Not Found")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var act = async () => await _apiClient.GetAsync<object>("https://test.com/api");
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetStringAsync_WithValidUrl_ReturnsContent()
    {
        // Arrange
        var expectedContent = "Test Content";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedContent)
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _apiClient.GetStringAsync("https://test.com/api");

        // Assert
        result.Should().Be(expectedContent);
    }

    [Fact]
    public async Task GetStringAsync_WithNullUrl_ThrowsArgumentException()
    {
        // Act & Assert
        var act = async () => await _apiClient.GetStringAsync(null!);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task GetStringAsync_WithHttpError_ThrowsHttpRequestException()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Server Error")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        var act = async () => await _apiClient.GetStringAsync("https://test.com/api");
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
