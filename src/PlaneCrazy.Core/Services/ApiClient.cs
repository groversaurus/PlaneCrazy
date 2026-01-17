using System.Text.Json;

namespace PlaneCrazy.Core.Services;

/// <summary>
/// Base HTTP client for making API requests
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Performs a GET request and deserializes the response
    /// </summary>
    /// <typeparam name="T">Type to deserialize the response to</typeparam>
    /// <param name="url">URL to request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response</returns>
    public virtual async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    /// <summary>
    /// Performs a GET request and returns the raw response content
    /// </summary>
    /// <param name="url">URL to request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response content as string</returns>
    public virtual async Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
