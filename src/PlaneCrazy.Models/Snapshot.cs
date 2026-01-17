namespace PlaneCrazy.Models;

/// <summary>
/// Represents a timestamped snapshot of aircraft state data.
/// Combines aircraft information with position data at a specific moment in time.
/// </summary>
public class Snapshot
{
    /// <summary>
    /// UTC timestamp when this snapshot was captured.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Aircraft identification and flight information.
    /// </summary>
    public Aircraft Aircraft { get; set; } = new();

    /// <summary>
    /// Aircraft position data (latitude, longitude, altitude).
    /// </summary>
    public Position Position { get; set; } = new();

    /// <summary>
    /// Time of last position update (Unix timestamp in seconds).
    /// </summary>
    public long? SeenPos { get; set; }

    /// <summary>
    /// Time of last message received (Unix timestamp in seconds).
    /// </summary>
    public long? Seen { get; set; }

    /// <summary>
    /// Number of messages received from this aircraft.
    /// </summary>
    public int? Messages { get; set; }

    /// <summary>
    /// Received Signal Strength Indicator (RSSI) in dBFS.
    /// </summary>
    public double? Rssi { get; set; }
}
