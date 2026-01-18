using PlaneCrazy.Domain.Events;

namespace PlaneCrazy.Tests.Helpers;

/// <summary>
/// Helper methods for creating test events and test data.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a CommentAdded event with default or custom values.
    /// </summary>
    public static CommentAdded CreateCommentAddedEvent(
        string entityType = "Aircraft",
        string entityId = "A12345",
        string text = "Test comment",
        Guid? commentId = null,
        string? user = "testuser",
        DateTime? timestamp = null)
    {
        return new CommentAdded
        {
            CommentId = commentId ?? Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Text = text,
            User = user,
            Timestamp = timestamp ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a CommentEdited event with default or custom values.
    /// </summary>
    public static CommentEdited CreateCommentEditedEvent(
        Guid commentId,
        string entityType = "Aircraft",
        string entityId = "A12345",
        string text = "Edited comment",
        string? previousText = "Original comment",
        string? user = "testuser",
        DateTime? timestamp = null)
    {
        return new CommentEdited
        {
            CommentId = commentId,
            EntityType = entityType,
            EntityId = entityId,
            Text = text,
            PreviousText = previousText,
            User = user,
            Timestamp = timestamp ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a CommentDeleted event with default or custom values.
    /// </summary>
    public static CommentDeleted CreateCommentDeletedEvent(
        Guid commentId,
        string entityType = "Aircraft",
        string entityId = "A12345",
        string? reason = "Test deletion",
        string? user = "testuser",
        DateTime? timestamp = null)
    {
        return new CommentDeleted
        {
            CommentId = commentId,
            EntityType = entityType,
            EntityId = entityId,
            Reason = reason,
            User = user,
            Timestamp = timestamp ?? DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an AircraftFavourited event with default or custom values.
    /// </summary>
    public static AircraftFavourited CreateAircraftFavouritedEvent(
        string icao24 = "ABC123",
        string? registration = "N12345",
        string? typeCode = "B738")
    {
        return new AircraftFavourited
        {
            Icao24 = icao24,
            Registration = registration,
            TypeCode = typeCode
        };
    }

    /// <summary>
    /// Creates an AircraftUnfavourited event with default or custom values.
    /// </summary>
    public static AircraftUnfavourited CreateAircraftUnfavouritedEvent(
        string icao24 = "ABC123")
    {
        return new AircraftUnfavourited
        {
            Icao24 = icao24
        };
    }

    /// <summary>
    /// Creates a TypeFavourited event with default or custom values.
    /// </summary>
    public static TypeFavourited CreateTypeFavouritedEvent(
        string typeCode = "B738",
        string? typeName = "Boeing 737-800")
    {
        return new TypeFavourited
        {
            TypeCode = typeCode,
            TypeName = typeName
        };
    }

    /// <summary>
    /// Creates a TypeUnfavourited event with default or custom values.
    /// </summary>
    public static TypeUnfavourited CreateTypeUnfavouritedEvent(
        string typeCode = "B738")
    {
        return new TypeUnfavourited
        {
            TypeCode = typeCode
        };
    }

    /// <summary>
    /// Creates an AirportFavourited event with default or custom values.
    /// </summary>
    public static AirportFavourited CreateAirportFavouritedEvent(
        string icaoCode = "KJFK",
        string? name = "John F. Kennedy International Airport")
    {
        return new AirportFavourited
        {
            IcaoCode = icaoCode,
            Name = name
        };
    }

    /// <summary>
    /// Creates an AirportUnfavourited event with default or custom values.
    /// </summary>
    public static AirportUnfavourited CreateAirportUnfavouritedEvent(
        string icaoCode = "KJFK")
    {
        return new AirportUnfavourited
        {
            IcaoCode = icaoCode
        };
    }

    /// <summary>
    /// Creates an AircraftFirstSeen event with default values.
    /// </summary>
    public static AircraftFirstSeen CreateAircraftFirstSeenEvent(
        string icao24 = "ABC123",
        double? initialLatitude = 40.7128,
        double? initialLongitude = -74.0060)
    {
        return new AircraftFirstSeen
        {
            Icao24 = icao24,
            InitialLatitude = initialLatitude,
            InitialLongitude = initialLongitude
        };
    }

    /// <summary>
    /// Creates an AircraftPositionUpdated event with default values.
    /// </summary>
    public static AircraftPositionUpdated CreateAircraftPositionUpdatedEvent(
        string icao24 = "ABC123",
        double latitude = 40.7128,
        double longitude = -74.0060,
        double? altitude = 35000)
    {
        return new AircraftPositionUpdated
        {
            Icao24 = icao24,
            Latitude = latitude,
            Longitude = longitude,
            Altitude = altitude
        };
    }

    /// <summary>
    /// Adds a delay to ensure distinct timestamps for events.
    /// </summary>
    public static async Task DelayForDistinctTimestamp()
    {
        await Task.Delay(10); // 10ms delay
    }
}
