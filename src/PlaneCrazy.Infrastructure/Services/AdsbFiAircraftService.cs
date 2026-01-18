using System.Net.Http.Json;
using PlaneCrazy.Domain.Entities;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.Models.AdsbFi;

namespace PlaneCrazy.Infrastructure.Services;

public class AdsbFiAircraftService : IAircraftDataService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.adsb.fi/v2";

    public AdsbFiAircraftService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Aircraft>> FetchAircraftAsync()
    {
        try
        {
            // Fetch aircraft in a bounding box (example: roughly Europe)
            var lat1 = 35.0;
            var lon1 = -10.0;
            var lat2 = 70.0;
            var lon2 = 40.0;

            var url = $"{BaseUrl}/lat/{lat1}/lon/{lon1}/lat/{lat2}/lon/{lon2}";
            var response = await _httpClient.GetFromJsonAsync<AdsbFiSnapshot>(url);

            if (response?.Aircraft == null)
            {
                return Enumerable.Empty<Aircraft>();
            }

            return response.Aircraft.Select(MapToAircraft).Where(a => a != null).Cast<Aircraft>();
        }
        catch
        {
            // Return empty list on error
            return Enumerable.Empty<Aircraft>();
        }
    }

    private Aircraft? MapToAircraft(AdsbFiAircraft adsbAircraft)
    {
        if (string.IsNullOrEmpty(adsbAircraft.Hex))
        {
            return null;
        }

        return new Aircraft
        {
            Icao24 = adsbAircraft.Hex.ToUpper(),
            Registration = adsbAircraft.R,
            TypeCode = adsbAircraft.T,
            Latitude = adsbAircraft.Lat,
            Longitude = adsbAircraft.Lon,
            Altitude = adsbAircraft.Alt_Baro,
            Velocity = adsbAircraft.Gs,
            Track = adsbAircraft.Track,
            OnGround = false, // Default to false; proper ground detection would require API flag
            Callsign = adsbAircraft.Flight?.Trim(),
            LastSeen = DateTime.UtcNow
        };
    }


}
