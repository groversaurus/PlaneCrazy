using PlaneCrazy.Core.Models;
using PlaneCrazy.Core.Services;

namespace PlaneCrazy.Core.Repositories;

/// <summary>
/// Repository implementation for adsb.fi API
/// </summary>
public class AdsBFiRepository : IAdsBFiRepository
{
    private readonly ApiClient _apiClient;
    private readonly string _baseUrl;

    /// <summary>
    /// Initializes a new instance of the AdsBFiRepository
    /// </summary>
    /// <param name="apiClient">API client for making HTTP requests</param>
    /// <param name="baseUrl">Base URL for adsb.fi API (default: https://api.adsb.fi/v2)</param>
    public AdsBFiRepository(ApiClient apiClient, string? baseUrl = null)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _baseUrl = baseUrl ?? "https://api.adsb.fi/v2";
    }

    /// <inheritdoc />
    public async Task<AircraftResponse?> GetAllAircraftAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl}/all";
        return await _apiClient.GetAsync<AircraftResponse>(url, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AircraftResponse?> GetAircraftByLocationAsync(
        double latitude, 
        double longitude, 
        double radiusNm, 
        CancellationToken cancellationToken = default)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90");
        }

        if (longitude < -180 || longitude > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180");
        }

        if (radiusNm <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radiusNm), "Radius must be greater than 0");
        }

        var url = $"{_baseUrl}/lat/{latitude}/lon/{longitude}/dist/{radiusNm}";
        return await _apiClient.GetAsync<AircraftResponse>(url, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Aircraft?> GetAircraftByHexAsync(string hex, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            throw new ArgumentException("Hex code cannot be null or empty", nameof(hex));
        }

        var url = $"{_baseUrl}/hex/{hex}";
        var response = await _apiClient.GetAsync<AircraftResponse>(url, cancellationToken);
        
        // The API returns an AircraftResponse with a single aircraft or empty list
        return response?.Aircraft.FirstOrDefault();
    }
}
