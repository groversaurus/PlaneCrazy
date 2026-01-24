using PlaneCrazy.Domain.Entities;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.Models.AdsbFi;

namespace PlaneCrazy.Infrastructure.Services;

public class AdsbFiAircraftService : IAircraftDataService
{
    private readonly IApiClient _apiClient;
    private readonly ActiveAirportService _activeAirportService;
    private const string BaseUrl = "https://opendata.adsb.fi/api/v3";

    public AdsbFiAircraftService(IApiClient apiClient, ActiveAirportService activeAirportService)
    {
        _apiClient = apiClient;
        _activeAirportService = activeAirportService;
    }

    public async Task<IEnumerable<Aircraft>> FetchAircraftAsync()
    {
        try
        {
            // Use active airport if set, otherwise default to Manchester (EGCC)
            var active = _activeAirportService.GetActiveAirport();
            double lat, lon, distKm;
            const double maxNm = 100.0;
            const double nmToKm = 1.852;
            if (active != null)
            {
                lat = active.Latitude;
                lon = active.Longitude;
                // Convert radius from degrees to km (1 deg ≈ 111km), but clamp to 100nm (185km)
                distKm = Math.Min(Math.Max(active.RadiusDegrees * 111, 10), maxNm * nmToKm);
            }
            else
            {
                // Default: Manchester, 100nm
                lat = 53.3537;
                lon = -2.275;
                distKm = maxNm * nmToKm;
            }

            var url = $"{BaseUrl}/lat/{lat:F4}/lon/{lon:F4}/dist/{distKm:F0}";
            var response = await _apiClient.GetAsync<AdsbFiSnapshot>(url);

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
