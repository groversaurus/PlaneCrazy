namespace PlaneCrazy.Domain.Interfaces;

/// <summary>
/// Defines a contract for making HTTP REST API calls.
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Makes an HTTP GET request to the specified URL and deserializes the JSON response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="url">The URL to send the GET request to.</param>
    /// <returns>The deserialized response object, or null if the request fails.</returns>
    Task<T?> GetAsync<T>(string url);
}
