namespace PlaneCrazy.Models;

/// <summary>
/// Emitter category indicating the type of aircraft or vehicle.
/// Based on ADS-B emitter category classifications.
/// </summary>
public enum EmitterCategory
{
    /// <summary>
    /// No aircraft type information available.
    /// </summary>
    None = 0,

    /// <summary>
    /// Light aircraft (less than 15,500 lbs).
    /// </summary>
    Light = 1,

    /// <summary>
    /// Small aircraft (15,500 to 75,000 lbs).
    /// </summary>
    Small = 2,

    /// <summary>
    /// Large aircraft (75,000 to 300,000 lbs).
    /// </summary>
    Large = 3,

    /// <summary>
    /// High vortex large aircraft (e.g., B757).
    /// </summary>
    HighVortexLarge = 4,

    /// <summary>
    /// Heavy aircraft (greater than 300,000 lbs).
    /// </summary>
    Heavy = 5,

    /// <summary>
    /// High performance aircraft (>5g acceleration and >400 knots).
    /// </summary>
    HighPerformance = 6,

    /// <summary>
    /// Rotorcraft (helicopters).
    /// </summary>
    Rotorcraft = 7,

    /// <summary>
    /// Glider or sailplane.
    /// </summary>
    Glider = 8,

    /// <summary>
    /// Lighter-than-air (balloons, blimps).
    /// </summary>
    LighterThanAir = 9,

    /// <summary>
    /// Parachutist or skydiver.
    /// </summary>
    Parachutist = 10,

    /// <summary>
    /// Ultralight, hang glider, paraglider.
    /// </summary>
    Ultralight = 11,

    /// <summary>
    /// Unmanned aerial vehicle (UAV/drone).
    /// </summary>
    UAV = 12,

    /// <summary>
    /// Space or trans-atmospheric vehicle.
    /// </summary>
    Space = 13,

    /// <summary>
    /// Surface vehicle (emergency, service, or ground vehicle).
    /// </summary>
    SurfaceEmergency = 14,

    /// <summary>
    /// Surface vehicle (service vehicle).
    /// </summary>
    SurfaceService = 15,

    /// <summary>
    /// Point obstacle (includes tethered balloons).
    /// </summary>
    PointObstacle = 16,

    /// <summary>
    /// Cluster obstacle.
    /// </summary>
    ClusterObstacle = 17,

    /// <summary>
    /// Line obstacle.
    /// </summary>
    LineObstacle = 18,

    /// <summary>
    /// Reserved for future use.
    /// </summary>
    Reserved = 19
}
