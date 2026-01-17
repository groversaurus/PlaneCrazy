using PlaneCrazy.Core.Events;

namespace PlaneCrazy.Core.Comments;

public class CommentAddedEvent : BaseEvent
{
    public Guid CommentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}
