namespace PlaneCrazy.Domain.Queries.QueryResults;

/// <summary>
/// Detailed aircraft result with comments and favourite status.
/// </summary>
public class AircraftWithDetailsQueryResult
{
    public required AircraftQueryResult Aircraft { get; init; }
    public List<CommentQueryResult> Comments { get; init; } = new();
    public bool IsFavourited { get; init; }
    public DateTime? FavouritedAt { get; init; }
}
