namespace PlaneCrazy.Domain.Queries.QueryResults;

/// <summary>
/// Read-optimized result for comment queries.
/// </summary>
public class CommentQueryResult
{
    public Guid Id { get; init; }
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public required string Text { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? UpdatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsDeleted { get; init; }
    public string? DeletedBy { get; init; }
    public DateTime? DeletedAt { get; init; }
}
