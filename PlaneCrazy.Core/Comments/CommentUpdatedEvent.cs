using PlaneCrazy.Core.Events;

namespace PlaneCrazy.Core.Comments;

public class CommentUpdatedEvent : BaseEvent
{
    public Guid CommentId { get; init; }
    public string Text { get; init; } = string.Empty;
}
