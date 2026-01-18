using PlaneCrazy.Domain.Entities;
using PlaneCrazy.Domain.Commands;

namespace PlaneCrazy.Infrastructure.Repositories;

public class FavouriteRepository : JsonFileRepository<Favourite>
{
    public FavouriteRepository() 
        : base("favourites.json")
    {
    }

    protected override string GetEntityId(Favourite entity) => $"{entity.EntityType}_{entity.EntityId}";
    
    public async Task<IEnumerable<Favourite>> GetByEntityTypeAsync(string entityType)
    {
        var all = await GetAllAsync();
        return all.Where(f => f.EntityType == entityType);
    }
    
    /// <summary>
    /// Adds an aircraft to favourites.
    /// </summary>
    /// <param name="icao24">The ICAO24 hex identifier of the aircraft.</param>
    /// <param name="registration">The aircraft registration (optional).</param>
    /// <param name="typeCode">The aircraft type code (optional).</param>
    /// <param name="user">The user favouriting the aircraft (optional).</param>
    public async Task FavouriteAircraftAsync(string icao24, string? registration = null, string? typeCode = null, string? user = null)
    {
        var favourite = new Favourite
        {
            EntityType = "Aircraft",
            EntityId = icao24,
            Metadata = new Dictionary<string, string>()
        };
        
        if (!string.IsNullOrWhiteSpace(registration))
            favourite.Metadata["Registration"] = registration;
        
        if (!string.IsNullOrWhiteSpace(typeCode))
            favourite.Metadata["TypeCode"] = typeCode;
        
        if (!string.IsNullOrWhiteSpace(user))
            favourite.Metadata["User"] = user;
        
        await SaveAsync(favourite);
    }
    
    /// <summary>
    /// Removes an aircraft from favourites.
    /// </summary>
    /// <param name="icao24">The ICAO24 hex identifier of the aircraft.</param>
    public async Task UnfavouriteAircraftAsync(string icao24)
    {
        await DeleteAsync($"Aircraft_{icao24}");
    }
    
    /// <summary>
    /// Adds an aircraft type to favourites.
    /// </summary>
    /// <param name="typeCode">The aircraft type code.</param>
    /// <param name="typeName">The aircraft type name (optional).</param>
    /// <param name="user">The user favouriting the type (optional).</param>
    public async Task FavouriteTypeAsync(string typeCode, string? typeName = null, string? user = null)
    {
        var favourite = new Favourite
        {
            EntityType = "Type",
            EntityId = typeCode,
            Metadata = new Dictionary<string, string>()
        };
        
        if (!string.IsNullOrWhiteSpace(typeName))
            favourite.Metadata["TypeName"] = typeName;
        
        if (!string.IsNullOrWhiteSpace(user))
            favourite.Metadata["User"] = user;
        
        await SaveAsync(favourite);
    }
    
    /// <summary>
    /// Removes an aircraft type from favourites.
    /// </summary>
    /// <param name="typeCode">The aircraft type code.</param>
    public async Task UnfavouriteTypeAsync(string typeCode)
    {
        await DeleteAsync($"Type_{typeCode}");
    }
    
    /// <summary>
    /// Adds an airport to favourites.
    /// </summary>
    /// <param name="icaoCode">The ICAO airport code.</param>
    /// <param name="name">The airport name (optional).</param>
    /// <param name="user">The user favouriting the airport (optional).</param>
    public async Task FavouriteAirportAsync(string icaoCode, string? name = null, string? user = null)
    {
        var favourite = new Favourite
        {
            EntityType = "Airport",
            EntityId = icaoCode,
            Metadata = new Dictionary<string, string>()
        };
        
        if (!string.IsNullOrWhiteSpace(name))
            favourite.Metadata["Name"] = name;
        
        if (!string.IsNullOrWhiteSpace(user))
            favourite.Metadata["User"] = user;
        
        await SaveAsync(favourite);
    }
    
    /// <summary>
    /// Removes an airport from favourites.
    /// </summary>
    /// <param name="icaoCode">The ICAO airport code.</param>
    public async Task UnfavouriteAirportAsync(string icaoCode)
    {
        await DeleteAsync($"Airport_{icaoCode}");
    }
    
    /// <summary>
    /// Favourites an aircraft using a command object.
    /// </summary>
    public async Task FavouriteAircraftAsync(FavouriteAircraftCommand command)
    {
        command.Validate();
        
        await FavouriteAircraftAsync(
            command.Icao24,
            command.Registration,
            command.TypeCode,
            command.User ?? command.IssuedBy
        );
    }
    
    /// <summary>
    /// Unfavourites an aircraft using a command object.
    /// </summary>
    public async Task UnfavouriteAircraftAsync(UnfavouriteAircraftCommand command)
    {
        command.Validate();
        
        await UnfavouriteAircraftAsync(command.Icao24);
    }
    
    /// <summary>
    /// Favourites an aircraft type using a command object.
    /// </summary>
    public async Task FavouriteTypeAsync(FavouriteAircraftTypeCommand command)
    {
        command.Validate();
        
        await FavouriteTypeAsync(
            command.TypeCode,
            command.TypeName,
            command.User ?? command.IssuedBy
        );
    }
    
    /// <summary>
    /// Unfavourites an aircraft type using a command object.
    /// </summary>
    public async Task UnfavouriteTypeAsync(UnfavouriteAircraftTypeCommand command)
    {
        command.Validate();
        
        await UnfavouriteTypeAsync(command.TypeCode);
    }
    
    /// <summary>
    /// Favourites an airport using a command object.
    /// </summary>
    public async Task FavouriteAirportAsync(FavouriteAirportCommand command)
    {
        command.Validate();
        
        await FavouriteAirportAsync(
            command.IcaoCode,
            command.Name,
            command.User ?? command.IssuedBy
        );
    }
    
    /// <summary>
    /// Unfavourites an airport using a command object.
    /// </summary>
    public async Task UnfavouriteAirportAsync(UnfavouriteAirportCommand command)
    {
        command.Validate();
        
        await UnfavouriteAirportAsync(command.IcaoCode);
    }
}
