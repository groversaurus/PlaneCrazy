using PlaneCrazy.Domain.Entities;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Domain.Queries.QueryResults;
using PlaneCrazy.Infrastructure.Repositories;

namespace PlaneCrazy.Infrastructure.QueryServices;

/// <summary>
/// Query service for aircraft read operations using projections only.
/// </summary>
public class AircraftQueryService : IAircraftQueryService
{
    private readonly AircraftRepository _aircraftRepository;
    private readonly CommentRepository _commentRepository;
    private readonly FavouriteRepository _favouriteRepository;
    private readonly AirportRepository _airportRepository;

    public AircraftQueryService(
        AircraftRepository aircraftRepository,
        CommentRepository commentRepository,
        FavouriteRepository favouriteRepository,
        AirportRepository airportRepository)
    {
        _aircraftRepository = aircraftRepository;
        _commentRepository = commentRepository;
        _favouriteRepository = favouriteRepository;
        _airportRepository = airportRepository;
    }

    public async Task<AircraftQueryResult?> GetByHexAsync(string icao24)
    {
        var aircraft = await _aircraftRepository.GetByIdAsync(icao24);
        if (aircraft == null) return null;

        return await MapToQueryResultAsync(aircraft);
    }

    public async Task<IEnumerable<AircraftQueryResult>> GetAllAircraftAsync()
    {
        var aircraft = await _aircraftRepository.GetAllAsync();
        
        // Batch load favourites and comments to avoid N+1 queries
        var allFavourites = await _favouriteRepository.GetAllAsync();
        var allComments = await _commentRepository.GetAllAsync();
        
        var favouritedAircraftIds = new HashSet<string>(
            allFavourites
                .Where(f => f.EntityType == "Aircraft")
                .Select(f => f.EntityId)
        );
        
        var commentsByAircraft = allComments
            .Where(c => c.EntityType == "Aircraft" && !c.IsDeleted)
            .GroupBy(c => c.EntityId)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var results = new List<AircraftQueryResult>();

        foreach (var plane in aircraft)
        {
            results.Add(MapToQueryResultWithCache(plane, favouritedAircraftIds, commentsByAircraft));
        }

        return results;
    }

    public async Task<IEnumerable<AircraftQueryResult>> GetAircraftWithEnrichedDataAsync()
    {
        return await GetAllAircraftAsync(); // Already enriched in mapping
    }

    public async Task<AircraftWithDetailsQueryResult?> GetAircraftWithDetailsAsync(string icao24)
    {
        var aircraftResult = await GetByHexAsync(icao24);
        if (aircraftResult == null) return null;

        var comments = await _commentRepository.GetActiveByEntityAsync("Aircraft", icao24);
        var commentResults = comments.Select(MapCommentToQueryResult).ToList();

        var favourite = await _favouriteRepository.GetByIdAsync($"Aircraft_{icao24}");

        return new AircraftWithDetailsQueryResult
        {
            Aircraft = aircraftResult,
            Comments = commentResults,
            IsFavourited = favourite != null,
            FavouritedAt = favourite?.FavouritedAt
        };
    }

    public async Task<IEnumerable<AircraftQueryResult>> GetAircraftWithCommentsAsync()
    {
        var allComments = await _commentRepository.GetAllAsync();
        var aircraftWithComments = allComments
            .Where(c => c.EntityType == "Aircraft" && !c.IsDeleted)
            .Select(c => c.EntityId)
            .Distinct()
            .ToList();

        var allAircraft = await _aircraftRepository.GetAllAsync();
        var aircraftMap = allAircraft.ToDictionary(a => a.Icao24);
        
        // Batch load favourites and comments to avoid N+1 queries
        var allFavourites = await _favouriteRepository.GetAllAsync();
        var favouritedAircraftIds = new HashSet<string>(
            allFavourites
                .Where(f => f.EntityType == "Aircraft")
                .Select(f => f.EntityId)
        );
        
        var commentsByAircraft = allComments
            .Where(c => c.EntityType == "Aircraft" && !c.IsDeleted)
            .GroupBy(c => c.EntityId)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var results = new List<AircraftQueryResult>();

        foreach (var icao24 in aircraftWithComments)
        {
            if (aircraftMap.TryGetValue(icao24, out var aircraft))
            {
                results.Add(MapToQueryResultWithCache(aircraft, favouritedAircraftIds, commentsByAircraft));
            }
        }

        return results;
    }

    public async Task<IEnumerable<AirportNearbyAircraftQueryResult>> GetAircraftNearFavouriteAirportsAsync(
        double radiusNauticalMiles = 50)
    {
        var favouriteAirports = await _favouriteRepository.GetByEntityTypeAsync("Airport");
        var allAircraft = await _aircraftRepository.GetAllAsync();
        
        // Batch load all data to avoid N+1 queries
        var allFavourites = await _favouriteRepository.GetAllAsync();
        var allComments = await _commentRepository.GetAllAsync();
        
        var favouritedAircraftIds = new HashSet<string>(
            allFavourites
                .Where(f => f.EntityType == "Aircraft")
                .Select(f => f.EntityId)
        );
        
        var commentsByAircraft = allComments
            .Where(c => c.EntityType == "Aircraft" && !c.IsDeleted)
            .GroupBy(c => c.EntityId)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var results = new List<AirportNearbyAircraftQueryResult>();

        foreach (var favAirport in favouriteAirports)
        {
            var airport = await _airportRepository.GetByIdAsync(favAirport.EntityId);
            if (airport?.Latitude == null || airport?.Longitude == null)
                continue;

            var nearbyAircraft = new List<AircraftDistanceQueryResult>();

            foreach (var aircraft in allAircraft)
            {
                if (aircraft.Latitude == null || aircraft.Longitude == null)
                    continue;

                var distance = CalculateDistance(
                    airport.Latitude.Value,
                    airport.Longitude.Value,
                    aircraft.Latitude.Value,
                    aircraft.Longitude.Value);

                if (distance <= radiusNauticalMiles)
                {
                    nearbyAircraft.Add(new AircraftDistanceQueryResult
                    {
                        Aircraft = MapToQueryResultWithCache(aircraft, favouritedAircraftIds, commentsByAircraft),
                        DistanceNauticalMiles = distance
                    });
                }
            }

            if (nearbyAircraft.Any())
            {
                results.Add(new AirportNearbyAircraftQueryResult
                {
                    AirportIcaoCode = airport.IcaoCode,
                    AirportName = airport.Name ?? favAirport.Metadata.GetValueOrDefault("Name", "Unknown"),
                    AirportLatitude = airport.Latitude,
                    AirportLongitude = airport.Longitude,
                    NearbyAircraft = nearbyAircraft.OrderBy(a => a.DistanceNauticalMiles).ToList()
                });
            }
        }

        return results;
    }

    public async Task<IEnumerable<AircraftQueryResult>> GetByCallsignAsync(string callsign)
    {
        var allAircraft = await _aircraftRepository.GetAllAsync();
        var filtered = allAircraft.Where(a => 
            a.Callsign != null && 
            a.Callsign.Equals(callsign, StringComparison.OrdinalIgnoreCase)).ToList();

        // Batch load favourites and comments to avoid N+1 queries
        var allFavourites = await _favouriteRepository.GetAllAsync();
        var allComments = await _commentRepository.GetAllAsync();
        
        var favouritedAircraftIds = new HashSet<string>(
            allFavourites
                .Where(f => f.EntityType == "Aircraft")
                .Select(f => f.EntityId)
        );
        
        var commentsByAircraft = allComments
            .Where(c => c.EntityType == "Aircraft" && !c.IsDeleted)
            .GroupBy(c => c.EntityId)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var results = new List<AircraftQueryResult>();
        foreach (var aircraft in filtered)
        {
            results.Add(MapToQueryResultWithCache(aircraft, favouritedAircraftIds, commentsByAircraft));
        }

        return results;
    }

    public async Task<IEnumerable<AircraftQueryResult>> GetByTypeCodeAsync(string typeCode)
    {
        var allAircraft = await _aircraftRepository.GetAllAsync();
        var filtered = allAircraft.Where(a => 
            a.TypeCode != null && 
            a.TypeCode.Equals(typeCode, StringComparison.OrdinalIgnoreCase)).ToList();

        // Batch load favourites and comments to avoid N+1 queries
        var allFavourites = await _favouriteRepository.GetAllAsync();
        var allComments = await _commentRepository.GetAllAsync();
        
        var favouritedAircraftIds = new HashSet<string>(
            allFavourites
                .Where(f => f.EntityType == "Aircraft")
                .Select(f => f.EntityId)
        );
        
        var commentsByAircraft = allComments
            .Where(c => c.EntityType == "Aircraft" && !c.IsDeleted)
            .GroupBy(c => c.EntityId)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var results = new List<AircraftQueryResult>();
        foreach (var aircraft in filtered)
        {
            results.Add(MapToQueryResultWithCache(aircraft, favouritedAircraftIds, commentsByAircraft));
        }

        return results;
    }

    // Helper methods

    private async Task<AircraftQueryResult> MapToQueryResultAsync(Aircraft aircraft)
    {
        var isFavourited = await _favouriteRepository.GetByIdAsync($"Aircraft_{aircraft.Icao24}") != null;
        var comments = await _commentRepository.GetActiveByEntityAsync("Aircraft", aircraft.Icao24);

        return new AircraftQueryResult
        {
            Icao24 = aircraft.Icao24,
            Registration = aircraft.Registration,
            TypeCode = aircraft.TypeCode,
            Callsign = aircraft.Callsign,
            Latitude = aircraft.Latitude,
            Longitude = aircraft.Longitude,
            Altitude = aircraft.Altitude,
            Velocity = aircraft.Velocity,
            Track = aircraft.Track,
            OnGround = aircraft.OnGround,
            LastSeen = aircraft.LastSeen,
            LastUpdated = aircraft.LastUpdated,
            TotalUpdates = aircraft.TotalUpdates,
            IsFavourited = isFavourited,
            CommentCount = comments.Count(),
            Origin = aircraft.Origin,
            Destination = aircraft.Destination
        };
    }

    private AircraftQueryResult MapToQueryResultWithCache(
        Aircraft aircraft, 
        HashSet<string> favouritedAircraftIds, 
        Dictionary<string, int> commentsByAircraft)
    {
        return new AircraftQueryResult
        {
            Icao24 = aircraft.Icao24,
            Registration = aircraft.Registration,
            TypeCode = aircraft.TypeCode,
            Callsign = aircraft.Callsign,
            Latitude = aircraft.Latitude,
            Longitude = aircraft.Longitude,
            Altitude = aircraft.Altitude,
            Velocity = aircraft.Velocity,
            Track = aircraft.Track,
            OnGround = aircraft.OnGround,
            LastSeen = aircraft.LastSeen,
            LastUpdated = aircraft.LastUpdated,
            TotalUpdates = aircraft.TotalUpdates,
            IsFavourited = favouritedAircraftIds.Contains(aircraft.Icao24),
            CommentCount = commentsByAircraft.GetValueOrDefault(aircraft.Icao24, 0),
            Origin = aircraft.Origin,
            Destination = aircraft.Destination
        };
    }

    private CommentQueryResult MapCommentToQueryResult(Comment comment)
    {
        return new CommentQueryResult
        {
            Id = comment.Id,
            EntityType = comment.EntityType,
            EntityId = comment.EntityId,
            Text = comment.Text,
            CreatedBy = comment.CreatedBy,
            CreatedAt = comment.CreatedAt,
            UpdatedBy = comment.UpdatedBy,
            UpdatedAt = comment.UpdatedAt,
            IsDeleted = comment.IsDeleted,
            DeletedBy = comment.DeletedBy,
            DeletedAt = comment.DeletedAt
        };
    }

    /// <summary>
    /// Calculates great circle distance between two points using Haversine formula.
    /// Returns distance in nautical miles.
    /// </summary>
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusNm = 3440.065; // Earth radius in nautical miles

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusNm * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
