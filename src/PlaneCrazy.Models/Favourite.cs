namespace PlaneCrazy.Models;

public class Favourite
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
