using System.Text.Json;

namespace PlaneCrazy.ApiClient;

/// <summary>
/// A client for making HTTP API requests with JSON deserialization support.
/// </summary>
public class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiClient"/> class with a new HttpClient.
    /// </summary>
    public ApiClient() : this(new HttpClient(), disposeHttpClient: true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiClient"/> class with a specified HttpClient.
    /// </summary>
    /// <param name="httpClient">The HttpClient to use for requests.</param>
    /// <param name="disposeHttpClient">Whether to dispose the HttpClient when this instance is disposed.</param>
    public ApiClient(HttpClient httpClient, bool disposeHttpClient = false)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    /// <summary>
    /// Sends a GET request to the specified URI and deserializes the JSON response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="requestUri">The URI to send the GET request to.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The deserialized response object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when requestUri is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
    /// <exception cref="JsonException">Thrown when the response cannot be deserialized.</exception>
    public async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        if (requestUri == null)
        {
            throw new ArgumentNullException(nameof(requestUri));
        }

        var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<T>(contentStream, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes the ApiClient and optionally the underlying HttpClient.
    /// </summary>
    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
