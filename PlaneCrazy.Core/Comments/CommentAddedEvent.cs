using PlaneCrazy.Core.Events;

namespace PlaneCrazy.Core.Comments;

public class CommentAddedEvent : BaseEvent
{
    public Guid CommentId { get; init; }
    public string Text { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
}
