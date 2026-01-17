namespace PlaneCrazy.Core.Comments;

public class Comment
{
    public Guid CommentId { get; init; }
    public string EntityId { get; init; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
