using PlaneCrazy.Core.Events;

namespace PlaneCrazy.Core.Comments;

public class CommentUpdatedEvent : BaseEvent
{
    public Guid CommentId { get; set; }
    public string Text { get; set; } = string.Empty;
}
