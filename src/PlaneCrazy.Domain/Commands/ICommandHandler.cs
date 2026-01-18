namespace PlaneCrazy.Domain.Commands;

/// <summary>
/// Interface for command handlers that execute commands.
/// </summary>
/// <typeparam name="TCommand">The type of command this handler processes.</typeparam>
/// <typeparam name="TResult">The result type returned after handling the command.</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : Command
{
    /// <summary>
    /// Handles the command and returns a result.
    /// </summary>
    Task<TResult> HandleAsync(TCommand command);
}

/// <summary>
/// Interface for command handlers that don't return a result.
/// </summary>
public interface ICommandHandler<in TCommand> where TCommand : Command
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    Task HandleAsync(TCommand command);
}
