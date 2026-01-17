using System.Net;
using System.Text.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace PlaneCrazy.ApiClient.Tests;

public class ApiClientTests
{
    [Fact]
    public async Task GetAsync_WithValidResponse_ReturnsDeserializedObject()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var testData = new TestModel { Id = 1, Name = "Test" };
        var json = JsonSerializer.Serialize(testData);
        
        mockHttp.When("http://test.com/api/data")
            .Respond("application/json", json);
        
        var httpClient = mockHttp.ToHttpClient();
        using var apiClient = new ApiClient(httpClient);
        
        // Act
        var result = await apiClient.GetAsync<TestModel>("http://test.com/api/data");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetAsync_WithNullUri_ThrowsArgumentNullException()
    {
        // Arrange
        using var apiClient = new ApiClient();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await apiClient.GetAsync<TestModel>(null!));
    }

    [Fact]
    public async Task GetAsync_WithHttpError_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://test.com/api/error")
            .Respond(HttpStatusCode.InternalServerError);
        
        var httpClient = mockHttp.ToHttpClient();
        using var apiClient = new ApiClient(httpClient);
        
        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await apiClient.GetAsync<TestModel>("http://test.com/api/error"));
    }

    [Fact]
    public async Task GetAsync_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://test.com/api/invalid")
            .Respond("application/json", "{ invalid json }");
        
        var httpClient = mockHttp.ToHttpClient();
        using var apiClient = new ApiClient(httpClient);
        
        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(async () =>
            await apiClient.GetAsync<TestModel>("http://test.com/api/invalid"));
    }

    [Fact]
    public async Task GetAsync_WithCancellationToken_CancelsRequest()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://test.com/api/slow")
            .Respond(async () =>
            {
                await Task.Delay(1000);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });
        
        var httpClient = mockHttp.ToHttpClient();
        using var apiClient = new ApiClient(httpClient);
        var cts = new CancellationTokenSource();
        
        // Act
        var task = apiClient.GetAsync<TestModel>("http://test.com/api/slow", cts.Token);
        cts.Cancel();
        
        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ApiClient(null!));
    }

    [Fact]
    public void Constructor_WithDefaultConstructor_CreatesInstance()
    {
        // Act
        using var apiClient = new ApiClient();
        
        // Assert
        Assert.NotNull(apiClient);
    }

    [Fact]
    public async Task Dispose_WithDisposeHttpClientTrue_DisposesHttpClient()
    {
        // Arrange
        var httpClient = new HttpClient();
        var apiClient = new ApiClient(httpClient, disposeHttpClient: true);
        
        // Act
        apiClient.Dispose();
        
        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await httpClient.GetAsync("http://test.com"));
    }

    [Fact]
    public async Task Dispose_WithDisposeHttpClientFalse_DoesNotDisposeHttpClient()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://test.com/api/test")
            .Respond("application/json", "{}");
        
        var httpClient = mockHttp.ToHttpClient();
        var apiClient = new ApiClient(httpClient, disposeHttpClient: false);
        
        // Act
        apiClient.Dispose();
        
        // Assert - HttpClient should still be usable
        var exception = await Record.ExceptionAsync(async () => await httpClient.GetAsync("http://test.com/api/test"));
        Assert.Null(exception);
        
        // Cleanup
        httpClient.Dispose();
    }

    [Fact]
    public async Task GetAsync_WithComplexObject_DeserializesCorrectly()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        var testData = new ComplexTestModel
        {
            Id = 42,
            Name = "Complex",
            NestedObject = new NestedModel { Value = "Nested" },
            Items = new List<string> { "Item1", "Item2" }
        };
        var json = JsonSerializer.Serialize(testData);
        
        mockHttp.When("http://test.com/api/complex")
            .Respond("application/json", json);
        
        var httpClient = mockHttp.ToHttpClient();
        using var apiClient = new ApiClient(httpClient);
        
        // Act
        var result = await apiClient.GetAsync<ComplexTestModel>("http://test.com/api/complex");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Complex", result.Name);
        Assert.NotNull(result.NestedObject);
        Assert.Equal("Nested", result.NestedObject.Value);
        Assert.NotNull(result.Items);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Item1", result.Items[0]);
    }

    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class ComplexTestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public NestedModel? NestedObject { get; set; }
        public List<string>? Items { get; set; }
    }

    private class NestedModel
    {
        public string Value { get; set; } = string.Empty;
    }
}
