namespace PlaneCrazy.Domain.Queries.QueryResults;

/// <summary>
/// Read-optimized result for favourite queries.
/// </summary>
public class FavouriteQueryResult
{
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public DateTime FavouritedAt { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
    
    // Enriched data
    public int CommentCount { get; init; }
    
    // Type-specific properties (populated from Metadata)
    public string? Registration { get; init; }
    public string? TypeCode { get; init; }
    public string? TypeName { get; init; }
    public string? Name { get; init; }
}
