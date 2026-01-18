using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Events;

namespace PlaneCrazy.Domain.Aggregates;

/// <summary>
/// Aggregate root for managing favourites (Aircraft, Types, Airports).
/// Maintains the state of a favourite entity and enforces business rules.
/// </summary>
public class FavouriteAggregate : AggregateRoot
{
    private string _entityType = string.Empty;
    private string _entityId = string.Empty;
    private bool _isFavourited;

    /// <summary>
    /// Creates a new favourite aggregate with a specific ID.
    /// </summary>
    /// <param name="entityType">The type of entity (Aircraft, Type, or Airport).</param>
    /// <param name="entityId">The unique identifier of the entity.</param>
    public FavouriteAggregate(string entityType, string entityId)
    {
        Id = $"{entityType}_{entityId}";
        _entityType = entityType;
        _entityId = entityId;
    }

    /// <summary>
    /// Creates a new favourite aggregate (for loading from history).
    /// </summary>
    public FavouriteAggregate()
    {
    }

    /// <summary>
    /// Gets whether this entity is currently favourited.
    /// </summary>
    public bool IsFavourited => _isFavourited;

    /// <summary>
    /// Handles the FavouriteAircraft command.
    /// </summary>
    public void FavouriteAircraft(FavouriteAircraftCommand command)
    {
        // Business rule: Cannot favourite an already favourited aircraft
        if (_isFavourited)
            throw new InvalidOperationException($"Aircraft {command.Icao24} is already favourited.");

        var @event = new AircraftFavourited
        {
            Icao24 = command.Icao24,
            Registration = command.Registration,
            TypeCode = command.TypeCode
        };

        ApplyChange(@event);
    }

    /// <summary>
    /// Handles the UnfavouriteAircraft command.
    /// </summary>
    public void UnfavouriteAircraft(UnfavouriteAircraftCommand command)
    {
        // Business rule: Cannot unfavourite an aircraft that is not favourited
        if (!_isFavourited)
            throw new InvalidOperationException($"Aircraft {command.Icao24} is not favourited.");

        var @event = new AircraftUnfavourited
        {
            Icao24 = command.Icao24
        };

        ApplyChange(@event);
    }

    /// <summary>
    /// Handles the FavouriteAircraftType command.
    /// </summary>
    public void FavouriteAircraftType(FavouriteAircraftTypeCommand command)
    {
        // Business rule: Cannot favourite an already favourited type
        if (_isFavourited)
            throw new InvalidOperationException($"Type {command.TypeCode} is already favourited.");

        var @event = new TypeFavourited
        {
            TypeCode = command.TypeCode,
            TypeName = command.TypeName
        };

        ApplyChange(@event);
    }

    /// <summary>
    /// Handles the UnfavouriteAircraftType command.
    /// </summary>
    public void UnfavouriteAircraftType(UnfavouriteAircraftTypeCommand command)
    {
        // Business rule: Cannot unfavourite a type that is not favourited
        if (!_isFavourited)
            throw new InvalidOperationException($"Type {command.TypeCode} is not favourited.");

        var @event = new TypeUnfavourited
        {
            TypeCode = command.TypeCode
        };

        ApplyChange(@event);
    }

    /// <summary>
    /// Handles the FavouriteAirport command.
    /// </summary>
    public void FavouriteAirport(FavouriteAirportCommand command)
    {
        // Business rule: Cannot favourite an already favourited airport
        if (_isFavourited)
            throw new InvalidOperationException($"Airport {command.IcaoCode} is already favourited.");

        var @event = new AirportFavourited
        {
            IcaoCode = command.IcaoCode,
            Name = command.Name
        };

        ApplyChange(@event);
    }

    /// <summary>
    /// Handles the UnfavouriteAirport command.
    /// </summary>
    public void UnfavouriteAirport(UnfavouriteAirportCommand command)
    {
        // Business rule: Cannot unfavourite an airport that is not favourited
        if (!_isFavourited)
            throw new InvalidOperationException($"Airport {command.IcaoCode} is not favourited.");

        var @event = new AirportUnfavourited
        {
            IcaoCode = command.IcaoCode
        };

        ApplyChange(@event);
    }

    /// <summary>
    /// Applies events to rebuild the aggregate state.
    /// </summary>
    protected override void Apply(DomainEvent @event)
    {
        switch (@event)
        {
            case AircraftFavourited aircraftFavourited:
                ApplyAircraftFavourited(aircraftFavourited);
                break;
            case AircraftUnfavourited aircraftUnfavourited:
                ApplyAircraftUnfavourited(aircraftUnfavourited);
                break;
            case TypeFavourited typeFavourited:
                ApplyTypeFavourited(typeFavourited);
                break;
            case TypeUnfavourited typeUnfavourited:
                ApplyTypeUnfavourited(typeUnfavourited);
                break;
            case AirportFavourited airportFavourited:
                ApplyAirportFavourited(airportFavourited);
                break;
            case AirportUnfavourited airportUnfavourited:
                ApplyAirportUnfavourited(airportUnfavourited);
                break;
        }
    }

    private void ApplyAircraftFavourited(AircraftFavourited @event)
    {
        _entityType = "Aircraft";
        _entityId = @event.Icao24;
        Id = $"{_entityType}_{_entityId}";
        _isFavourited = true;
    }

    private void ApplyAircraftUnfavourited(AircraftUnfavourited @event)
    {
        _isFavourited = false;
    }

    private void ApplyTypeFavourited(TypeFavourited @event)
    {
        _entityType = "Type";
        _entityId = @event.TypeCode;
        Id = $"{_entityType}_{_entityId}";
        _isFavourited = true;
    }

    private void ApplyTypeUnfavourited(TypeUnfavourited @event)
    {
        _isFavourited = false;
    }

    private void ApplyAirportFavourited(AirportFavourited @event)
    {
        _entityType = "Airport";
        _entityId = @event.IcaoCode;
        Id = $"{_entityType}_{_entityId}";
        _isFavourited = true;
    }

    private void ApplyAirportUnfavourited(AirportUnfavourited @event)
    {
        _isFavourited = false;
    }
}
