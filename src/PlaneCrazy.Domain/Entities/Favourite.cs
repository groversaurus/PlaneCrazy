namespace PlaneCrazy.Domain.Entities;

/// <summary>
/// Generic favourite entity for backwards compatibility.
/// Prefer using typed models: AircraftFavourite, AircraftTypeFavourite, or AirportFavourite.
/// </summary>
public class Favourite
{
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public DateTime FavouritedAt { get; init; } = DateTime.UtcNow;
    public Dictionary<string, string> Metadata { get; init; } = new();
    
    /// <summary>
    /// Converts this generic favourite to a strongly-typed model if possible.
    /// </summary>
    public object? ToTypedFavourite()
    {
        return EntityType switch
        {
            "Aircraft" => new AircraftFavourite
            {
                Icao24 = EntityId,
                Registration = Metadata.GetValueOrDefault("Registration"),
                TypeCode = Metadata.GetValueOrDefault("TypeCode"),
                FavouritedAt = FavouritedAt
            },
            "Type" => new AircraftTypeFavourite
            {
                TypeCode = EntityId,
                TypeName = Metadata.GetValueOrDefault("TypeName"),
                FavouritedAt = FavouritedAt
            },
            "Airport" => new AirportFavourite
            {
                IcaoCode = EntityId,
                Name = Metadata.GetValueOrDefault("Name"),
                FavouritedAt = FavouritedAt
            },
            _ => null
        };
    }
}
