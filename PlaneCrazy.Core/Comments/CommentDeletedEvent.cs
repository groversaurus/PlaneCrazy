using PlaneCrazy.Core.Events;

namespace PlaneCrazy.Core.Comments;

public class CommentDeletedEvent : BaseEvent
{
    public Guid CommentId { get; init; }
}
