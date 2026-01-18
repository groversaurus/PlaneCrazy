using System.Net.Http.Json;
using PlaneCrazy.Domain.Interfaces;

namespace PlaneCrazy.Infrastructure.Http;

/// <summary>
/// Provides HTTP REST API client functionality for making HTTP requests.
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the ApiClient class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for making requests.</param>
    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Makes an HTTP GET request to the specified URL and deserializes the JSON response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="url">The URL to send the GET request to.</param>
    /// <returns>The deserialized response object, or null if the request fails or deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown when the URL is null or empty.</exception>
    public async Task<T?> GetAsync<T>(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }

        return await _httpClient.GetFromJsonAsync<T>(url);
    }
}
