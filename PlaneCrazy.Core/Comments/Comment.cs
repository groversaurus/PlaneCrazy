namespace PlaneCrazy.Core.Comments;

public class Comment
{
    public Guid CommentId { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
