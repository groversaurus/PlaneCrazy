using PlaneCrazy.Domain.Interfaces;

namespace PlaneCrazy.Infrastructure.Services;

/// <summary>
/// In-memory airport lookup service with common airports.
/// </summary>
public class AirportLookupService : IAirportLookupService
{
    private readonly Dictionary<string, AirportInfo> _airports;

    public AirportLookupService()
    {
        _airports = new Dictionary<string, AirportInfo>(StringComparer.OrdinalIgnoreCase)
        {
            // United Kingdom
            ["EGLL"] = new AirportInfo { IcaoCode = "EGLL", Name = "London Heathrow", Latitude = 51.4700, Longitude = -0.4543, City = "London", Country = "United Kingdom" },
            ["EGKK"] = new AirportInfo { IcaoCode = "EGKK", Name = "London Gatwick", Latitude = 51.1481, Longitude = -0.1903, City = "London", Country = "United Kingdom" },
            ["EGSS"] = new AirportInfo { IcaoCode = "EGSS", Name = "London Stansted", Latitude = 51.8850, Longitude = 0.2350, City = "London", Country = "United Kingdom" },
            ["EGGW"] = new AirportInfo { IcaoCode = "EGGW", Name = "London Luton", Latitude = 51.8747, Longitude = -0.3683, City = "London", Country = "United Kingdom" },
            ["EGCC"] = new AirportInfo { IcaoCode = "EGCC", Name = "Manchester Airport", Latitude = 53.3537, Longitude = -2.2750, City = "Manchester", Country = "United Kingdom" },
            
            // United States
            ["KJFK"] = new AirportInfo { IcaoCode = "KJFK", Name = "John F. Kennedy International Airport", Latitude = 40.6413, Longitude = -73.7781, City = "New York", Country = "United States" },
            ["KLAX"] = new AirportInfo { IcaoCode = "KLAX", Name = "Los Angeles International Airport", Latitude = 33.9425, Longitude = -118.4081, City = "Los Angeles", Country = "United States" },
            ["KORD"] = new AirportInfo { IcaoCode = "KORD", Name = "Chicago O'Hare International Airport", Latitude = 41.9742, Longitude = -87.9073, City = "Chicago", Country = "United States" },
            ["KATL"] = new AirportInfo { IcaoCode = "KATL", Name = "Hartsfield-Jackson Atlanta International Airport", Latitude = 33.6407, Longitude = -84.4277, City = "Atlanta", Country = "United States" },
            ["KDFW"] = new AirportInfo { IcaoCode = "KDFW", Name = "Dallas/Fort Worth International Airport", Latitude = 32.8998, Longitude = -97.0403, City = "Dallas", Country = "United States" },
            
            // France
            ["LFPG"] = new AirportInfo { IcaoCode = "LFPG", Name = "Paris Charles de Gaulle Airport", Latitude = 49.0097, Longitude = 2.5479, City = "Paris", Country = "France" },
            ["LFPO"] = new AirportInfo { IcaoCode = "LFPO", Name = "Paris Orly Airport", Latitude = 48.7233, Longitude = 2.3794, City = "Paris", Country = "France" },
            
            // Germany
            ["EDDF"] = new AirportInfo { IcaoCode = "EDDF", Name = "Frankfurt Airport", Latitude = 50.0379, Longitude = 8.5622, City = "Frankfurt", Country = "Germany" },
            ["EDDM"] = new AirportInfo { IcaoCode = "EDDM", Name = "Munich Airport", Latitude = 48.3538, Longitude = 11.7861, City = "Munich", Country = "Germany" },
            ["EDDB"] = new AirportInfo { IcaoCode = "EDDB", Name = "Berlin Brandenburg Airport", Latitude = 52.3667, Longitude = 13.5033, City = "Berlin", Country = "Germany" },
            
            // Netherlands
            ["EHAM"] = new AirportInfo { IcaoCode = "EHAM", Name = "Amsterdam Airport Schiphol", Latitude = 52.3086, Longitude = 4.7639, City = "Amsterdam", Country = "Netherlands" },
            
            // Spain
            ["LEMD"] = new AirportInfo { IcaoCode = "LEMD", Name = "Madrid-Barajas Airport", Latitude = 40.4936, Longitude = -3.5668, City = "Madrid", Country = "Spain" },
            ["LEBL"] = new AirportInfo { IcaoCode = "LEBL", Name = "Barcelona-El Prat Airport", Latitude = 41.2971, Longitude = 2.0785, City = "Barcelona", Country = "Spain" },
            
            // Italy
            ["LIRF"] = new AirportInfo { IcaoCode = "LIRF", Name = "Rome Fiumicino Airport", Latitude = 41.8003, Longitude = 12.2389, City = "Rome", Country = "Italy" },
            ["LIMC"] = new AirportInfo { IcaoCode = "LIMC", Name = "Milan Malpensa Airport", Latitude = 45.6306, Longitude = 8.7281, City = "Milan", Country = "Italy" },
            
            // Ireland
            ["EIDW"] = new AirportInfo { IcaoCode = "EIDW", Name = "Dublin Airport", Latitude = 53.4213, Longitude = -6.2701, City = "Dublin", Country = "Ireland" },
            
            // Switzerland
            ["LSZH"] = new AirportInfo { IcaoCode = "LSZH", Name = "Zurich Airport", Latitude = 47.4647, Longitude = 8.5492, City = "Zurich", Country = "Switzerland" },
            
            // Belgium
            ["EBBR"] = new AirportInfo { IcaoCode = "EBBR", Name = "Brussels Airport", Latitude = 50.9014, Longitude = 4.4844, City = "Brussels", Country = "Belgium" },
            
            // Austria
            ["LOWW"] = new AirportInfo { IcaoCode = "LOWW", Name = "Vienna International Airport", Latitude = 48.1103, Longitude = 16.5697, City = "Vienna", Country = "Austria" },
            
            // Denmark
            ["EKCH"] = new AirportInfo { IcaoCode = "EKCH", Name = "Copenhagen Airport", Latitude = 55.6181, Longitude = 12.6561, City = "Copenhagen", Country = "Denmark" },
            
            // Sweden
            ["ESSA"] = new AirportInfo { IcaoCode = "ESSA", Name = "Stockholm Arlanda Airport", Latitude = 59.6519, Longitude = 17.9186, City = "Stockholm", Country = "Sweden" },
            
            // Norway
            ["ENGM"] = new AirportInfo { IcaoCode = "ENGM", Name = "Oslo Airport", Latitude = 60.1939, Longitude = 11.1004, City = "Oslo", Country = "Norway" },
        };
    }

    public Task<AirportInfo?> LookupAsync(string icaoCode)
    {
        _airports.TryGetValue(icaoCode, out var airport);
        return Task.FromResult(airport);
    }

    public Task<IEnumerable<AirportInfo>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<AirportInfo>>(_airports.Values);
    }
}
