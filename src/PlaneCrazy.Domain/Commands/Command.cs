namespace PlaneCrazy.Domain.Commands;

/// <summary>
/// Base class for all commands in the system.
/// Commands represent user intent to perform an action.
/// </summary>
public abstract class Command
{
    /// <summary>
    /// Unique identifier for this command instance.
    /// </summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// When the command was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// The user who issued the command.
    /// </summary>
    public string? IssuedBy { get; init; }
    
    /// <summary>
    /// Optional correlation ID for tracking related commands.
    /// </summary>
    public string? CorrelationId { get; init; }
    
    /// <summary>
    /// Validates the command. Throws exception if invalid.
    /// </summary>
    public abstract void Validate();
}
