using System.Text.Json;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.Repositories;

namespace PlaneCrazy.Infrastructure.Services;

/// <summary>
/// Service for managing the active airport used for bounding box queries.
/// </summary>
public class ActiveAirportService
{
    private readonly string _settingsPath;
    private ActiveAirportSettings? _settings;

    public ActiveAirportService()
    {
        _settingsPath = Path.Combine(PlaneCrazyPaths.BasePath, "active_airport.json");
        LoadSettings();
    }

    /// <summary>
    /// Gets the currently active airport, if any.
    /// </summary>
    public ActiveAirportSettings? GetActiveAirport()
    {
        return _settings;
    }

    /// <summary>
    /// Sets an airport as the active airport.
    /// </summary>
    public async Task SetActiveAirportAsync(string icaoCode, string name, double latitude, double longitude, double radiusDegrees = 5.0)
    {
        _settings = new ActiveAirportSettings
        {
            IcaoCode = icaoCode,
            Name = name,
            Latitude = latitude,
            Longitude = longitude,
            RadiusDegrees = radiusDegrees,
            SetAt = DateTime.UtcNow
        };

        await SaveSettingsAsync();
    }

    /// <summary>
    /// Clears the active airport (resets to default Europe-wide view).
    /// </summary>
    public async Task ClearActiveAirportAsync()
    {
        _settings = null;
        if (File.Exists(_settingsPath))
        {
            File.Delete(_settingsPath);
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets the bounding box for API queries based on the active airport or default.
    /// </summary>
    public (double minLat, double minLon, double maxLat, double maxLon) GetBoundingBox()
    {
        if (_settings != null)
        {
            // Create bounding box around the airport
            var radius = _settings.RadiusDegrees;
            return (
                _settings.Latitude - radius,
                _settings.Longitude - radius,
                _settings.Latitude + radius,
                _settings.Longitude + radius
            );
        }

        // Default: Europe-wide
        return (35.0, -10.0, 70.0, 40.0);
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                _settings = JsonSerializer.Deserialize<ActiveAirportSettings>(json);
            }
        }
        catch
        {
            _settings = null;
        }
    }

    private async Task SaveSettingsAsync()
    {
        Directory.CreateDirectory(PlaneCrazyPaths.BasePath);
        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsPath, json);
    }
}

public class ActiveAirportSettings
{
    public required string IcaoCode { get; set; }
    public required string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusDegrees { get; set; } = 5.0;
    public DateTime SetAt { get; set; }
}
