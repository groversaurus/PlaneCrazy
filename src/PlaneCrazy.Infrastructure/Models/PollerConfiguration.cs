namespace PlaneCrazy.Infrastructure.Models;

/// <summary>
/// Configuration options for the background ADS-B polling service.
/// </summary>
public class PollerConfiguration
{
    /// <summary>
    /// Whether the background poller is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Polling interval in seconds.
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Minimum latitude for the bounding box query.
    /// </summary>
    public double MinLatitude { get; set; } = 35.0;

    /// <summary>
    /// Minimum longitude for the bounding box query.
    /// </summary>
    public double MinLongitude { get; set; } = -10.0;

    /// <summary>
    /// Maximum latitude for the bounding box query.
    /// </summary>
    public double MaxLatitude { get; set; } = 70.0;

    /// <summary>
    /// Maximum longitude for the bounding box query.
    /// </summary>
    public double MaxLongitude { get; set; } = 40.0;

    /// <summary>
    /// Time in minutes after which an aircraft is considered "last seen" if not in the latest fetch.
    /// </summary>
    public int MissingAircraftTimeoutMinutes { get; set; } = 5;
}
