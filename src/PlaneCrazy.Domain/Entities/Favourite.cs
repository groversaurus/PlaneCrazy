namespace PlaneCrazy.Domain.Entities;

public class Favourite
{
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public DateTime FavouritedAt { get; init; } = DateTime.UtcNow;
    public Dictionary<string, string> Metadata { get; init; } = new();
}
