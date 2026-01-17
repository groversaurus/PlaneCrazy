namespace PlaneCrazy.Domain.Entities;

public class Comment
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public required string Text { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
