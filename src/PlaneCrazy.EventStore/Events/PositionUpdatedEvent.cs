namespace PlaneCrazy.EventStore.Events;

/// <summary>
/// Event raised when an aircraft's position is updated.
/// </summary>
public class PositionUpdatedEvent : EventBase
{
    /// <summary>
    /// Gets or sets the latitude.
    /// </summary>
    public double Latitude { get; set; }
    
    /// <summary>
    /// Gets or sets the longitude.
    /// </summary>
    public double Longitude { get; set; }
    
    /// <summary>
    /// Gets or sets the altitude in feet.
    /// </summary>
    public int Altitude { get; set; }
    
    /// <summary>
    /// Gets or sets the ground speed in knots.
    /// </summary>
    public double? GroundSpeed { get; set; }
    
    /// <summary>
    /// Gets or sets the track (heading) in degrees.
    /// </summary>
    public double? Track { get; set; }
    
    /// <summary>
    /// Gets or sets the vertical rate in feet per minute.
    /// </summary>
    public int? VerticalRate { get; set; }
}
