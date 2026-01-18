using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlaneCrazy.Domain.Entities;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.Models;
using PlaneCrazy.Infrastructure.Repositories;

namespace PlaneCrazy.Infrastructure.Services;

/// <summary>
/// Background service that periodically polls adsb.fi for aircraft data,
/// detects changes, and emits appropriate domain events.
/// </summary>
public class BackgroundAdsBPoller : IHostedService, IDisposable
{
    private readonly IAircraftDataService _aircraftDataService;
    private readonly IEventStore _eventStore;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly AircraftRepository _aircraftRepository;
    private readonly ILogger<BackgroundAdsBPoller> _logger;
    private readonly PollerConfiguration _config;
    private Timer? _timer;
    private bool _isPolling;

    public BackgroundAdsBPoller(
        IAircraftDataService aircraftDataService,
        IEventStore eventStore,
        IEventDispatcher eventDispatcher,
        AircraftRepository aircraftRepository,
        ILogger<BackgroundAdsBPoller> logger,
        PollerConfiguration config)
    {
        _aircraftDataService = aircraftDataService;
        _eventStore = eventStore;
        _eventDispatcher = eventDispatcher;
        _aircraftRepository = aircraftRepository;
        _logger = logger;
        _config = config;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("Background ADS-B poller is disabled in configuration");
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "Starting Background ADS-B poller with interval: {Interval} seconds",
            _config.PollingIntervalSeconds);

        _timer = new Timer(
            DoWork,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(_config.PollingIntervalSeconds));

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        // Prevent concurrent polling
        if (_isPolling)
        {
            _logger.LogDebug("Polling already in progress, skipping this cycle");
            return;
        }

        _isPolling = true;
        try
        {
            await PollAndProcessAircraftAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during aircraft polling");
        }
        finally
        {
            _isPolling = false;
        }
    }

    private async Task PollAndProcessAircraftAsync()
    {
        _logger.LogDebug("Fetching aircraft data from adsb.fi...");

        IEnumerable<Aircraft> fetchedAircraft;
        try
        {
            fetchedAircraft = await _aircraftDataService.FetchAircraftAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch aircraft data from adsb.fi");
            return;
        }

        var aircraftList = fetchedAircraft.ToList();
        _logger.LogInformation("Fetched {Count} aircraft from adsb.fi", aircraftList.Count);

        if (!aircraftList.Any())
        {
            _logger.LogWarning("No aircraft data fetched, skipping processing");
            return;
        }

        // Get all existing aircraft from repository and create dictionary for O(1) lookups
        var existingAircraftList = (await _aircraftRepository.GetAllAsync()).ToList();
        var existingAircraft = existingAircraftList.ToDictionary(a => a.Icao24);
        var fetchedIcaos = aircraftList.Select(a => a.Icao24).ToHashSet();

        int newCount = 0;
        int updatedCount = 0;

        // Process each fetched aircraft
        foreach (var aircraft in aircraftList)
        {
            if (existingAircraft.TryGetValue(aircraft.Icao24, out var existing))
            {
                // Existing aircraft - check for updates
                bool hasChanges = await ProcessExistingAircraftAsync(existing, aircraft);
                if (hasChanges)
                {
                    updatedCount++;
                }
            }
            else
            {
                // New aircraft
                await ProcessNewAircraftAsync(aircraft);
                newCount++;
            }
        }

        // Check for missing aircraft (in repository but not in latest fetch)
        var missingIcaos = existingAircraft.Keys.Except(fetchedIcaos).ToList();
        if (missingIcaos.Any())
        {
            await ProcessMissingAircraftAsync(missingIcaos, existingAircraft);
        }

        _logger.LogInformation(
            "Processing complete: {New} new, {Updated} updated, {Missing} missing aircraft",
            newCount, updatedCount, missingIcaos.Count);
    }

    private async Task ProcessNewAircraftAsync(Aircraft aircraft)
    {
        _logger.LogDebug("New aircraft detected: {Icao24}", aircraft.Icao24);

        var @event = new AircraftFirstSeen
        {
            Icao24 = aircraft.Icao24,
            FirstSeenAt = DateTime.UtcNow,
            InitialLatitude = aircraft.Latitude,
            InitialLongitude = aircraft.Longitude
        };

        await _eventDispatcher.DispatchAsync(@event);
        
        // Also emit identity and position events if data is available
        if (HasIdentityData(aircraft))
        {
            await EmitIdentityUpdateAsync(aircraft);
        }

        if (HasPositionData(aircraft))
        {
            await EmitPositionUpdateAsync(aircraft);
        }
    }

    private async Task<bool> ProcessExistingAircraftAsync(Aircraft existing, Aircraft fetched)
    {
        bool hasChanges = false;

        // Check for position changes
        if (HasPositionChanged(existing, fetched))
        {
            await EmitPositionUpdateAsync(fetched);
            hasChanges = true;
        }

        // Check for identity changes
        if (HasIdentityChanged(existing, fetched))
        {
            await EmitIdentityUpdateAsync(fetched);
            hasChanges = true;
        }

        // Always update LastSeen timestamp
        existing.LastSeen = DateTime.UtcNow;
        await _aircraftRepository.SaveAsync(existing);

        return hasChanges;
    }

    private async Task ProcessMissingAircraftAsync(List<string> missingIcaos, Dictionary<string, Aircraft> existingAircraft)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-_config.MissingAircraftTimeoutMinutes);

        foreach (var icao in missingIcaos)
        {
            var aircraft = existingAircraft[icao];
            
            // Only emit LastSeen event if aircraft hasn't been seen for more than the timeout period
            if (aircraft.LastSeen < cutoffTime)
            {
                _logger.LogDebug(
                    "Aircraft {Icao24} not seen for {Minutes} minutes, emitting LastSeen event",
                    icao, _config.MissingAircraftTimeoutMinutes);

                var @event = new AircraftLastSeen
                {
                    Icao24 = aircraft.Icao24,
                    LastSeenAt = aircraft.LastSeen,
                    LastLatitude = aircraft.Latitude,
                    LastLongitude = aircraft.Longitude,
                    LastAltitude = aircraft.Altitude
                };

                await _eventDispatcher.DispatchAsync(@event);
            }
        }
    }

    private async Task EmitPositionUpdateAsync(Aircraft aircraft)
    {
        var @event = new AircraftPositionUpdated
        {
            Icao24 = aircraft.Icao24,
            Latitude = aircraft.Latitude,
            Longitude = aircraft.Longitude,
            Altitude = aircraft.Altitude,
            Velocity = aircraft.Velocity,
            Track = aircraft.Track,
            VerticalRate = aircraft.VerticalRate,
            OnGround = aircraft.OnGround,
            Timestamp = DateTime.UtcNow
        };

        await _eventDispatcher.DispatchAsync(@event);
    }

    private async Task EmitIdentityUpdateAsync(Aircraft aircraft)
    {
        var @event = new AircraftIdentityUpdated
        {
            Icao24 = aircraft.Icao24,
            Registration = aircraft.Registration,
            TypeCode = aircraft.TypeCode,
            Callsign = aircraft.Callsign,
            Squawk = aircraft.Squawk,
            Origin = aircraft.Origin,
            Destination = aircraft.Destination,
            Timestamp = DateTime.UtcNow
        };

        await _eventDispatcher.DispatchAsync(@event);
    }

    private bool HasPositionChanged(Aircraft existing, Aircraft fetched)
    {
        return existing.Latitude != fetched.Latitude
            || existing.Longitude != fetched.Longitude
            || existing.Altitude != fetched.Altitude
            || existing.Velocity != fetched.Velocity
            || existing.Track != fetched.Track
            || existing.VerticalRate != fetched.VerticalRate
            || existing.OnGround != fetched.OnGround;
    }

    private bool HasIdentityChanged(Aircraft existing, Aircraft fetched)
    {
        return existing.Callsign != fetched.Callsign
            || existing.Registration != fetched.Registration
            || existing.TypeCode != fetched.TypeCode;
    }

    private bool HasPositionData(Aircraft aircraft)
    {
        return aircraft.Latitude.HasValue || aircraft.Longitude.HasValue || aircraft.Altitude.HasValue;
    }

    private bool HasIdentityData(Aircraft aircraft)
    {
        return !string.IsNullOrEmpty(aircraft.Callsign)
            || !string.IsNullOrEmpty(aircraft.Registration)
            || !string.IsNullOrEmpty(aircraft.TypeCode);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Background ADS-B poller");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
