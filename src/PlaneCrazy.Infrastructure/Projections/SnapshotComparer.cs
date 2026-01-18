using PlaneCrazy.Domain.Entities;
using PlaneCrazy.Domain.Models;

namespace PlaneCrazy.Infrastructure.Projections;

/// <summary>
/// Utility for comparing snapshots and detecting changes over time.
/// </summary>
public class SnapshotComparer
{
    /// <summary>
    /// Compares two snapshots and identifies changes.
    /// </summary>
    public SnapshotComparison Compare(AircraftSnapshot before, AircraftSnapshot after)
    {
        var beforeIcaos = before.Aircraft.Select(a => a.Icao24).ToHashSet();
        var afterIcaos = after.Aircraft.Select(a => a.Icao24).ToHashSet();

        var newAircraft = after.Aircraft
            .Where(a => !beforeIcaos.Contains(a.Icao24))
            .ToList();

        var removedAircraft = before.Aircraft
            .Where(a => !afterIcaos.Contains(a.Icao24))
            .ToList();

        var unchangedIcaos = beforeIcaos.Intersect(afterIcaos);
        var movedAircraft = new List<AircraftMovement>();

        foreach (var icao in unchangedIcaos)
        {
            var beforeAircraft = before.Aircraft.First(a => a.Icao24 == icao);
            var afterAircraft = after.Aircraft.First(a => a.Icao24 == icao);

            if (HasMoved(beforeAircraft, afterAircraft))
            {
                movedAircraft.Add(new AircraftMovement
                {
                    Aircraft = afterAircraft,
                    PreviousLatitude = beforeAircraft.Latitude,
                    PreviousLongitude = beforeAircraft.Longitude,
                    PreviousAltitude = beforeAircraft.Altitude,
                    CurrentLatitude = afterAircraft.Latitude,
                    CurrentLongitude = afterAircraft.Longitude,
                    CurrentAltitude = afterAircraft.Altitude
                });
            }
        }

        return new SnapshotComparison
        {
            BeforeSnapshot = before,
            AfterSnapshot = after,
            NewAircraft = newAircraft,
            RemovedAircraft = removedAircraft,
            MovedAircraft = movedAircraft
        };
    }

    private bool HasMoved(Aircraft before, Aircraft after)
    {
        return before.Latitude != after.Latitude
            || before.Longitude != after.Longitude
            || before.Altitude != after.Altitude;
    }
}

/// <summary>
/// Result of comparing two snapshots.
/// </summary>
public class SnapshotComparison
{
    public required AircraftSnapshot BeforeSnapshot { get; init; }
    public required AircraftSnapshot AfterSnapshot { get; init; }
    public required List<Aircraft> NewAircraft { get; init; }
    public required List<Aircraft> RemovedAircraft { get; init; }
    public required List<AircraftMovement> MovedAircraft { get; init; }

    public int TotalChanges => NewAircraft.Count + RemovedAircraft.Count + MovedAircraft.Count;
}

/// <summary>
/// Represents an aircraft that moved between snapshots.
/// </summary>
public class AircraftMovement
{
    public required Aircraft Aircraft { get; init; }
    public double? PreviousLatitude { get; init; }
    public double? PreviousLongitude { get; init; }
    public double? PreviousAltitude { get; init; }
    public double? CurrentLatitude { get; init; }
    public double? CurrentLongitude { get; init; }
    public double? CurrentAltitude { get; init; }
}
