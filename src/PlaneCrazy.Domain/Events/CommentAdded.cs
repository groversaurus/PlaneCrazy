namespace PlaneCrazy.Domain.Events;

public class CommentAdded : DomainEvent
{
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public required string CommentText { get; init; }
    public Guid CommentId { get; init; } = Guid.NewGuid();
}
