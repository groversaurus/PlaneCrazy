using PlaneCrazy.Domain.Aggregates;
using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.Projections;

namespace PlaneCrazy.Infrastructure.CommandHandlers;

/// <summary>
/// Command handler for adding comments to entities.
/// Loads event stream, rebuilds aggregate state, validates command, emits events, and updates projections.
/// </summary>
public class AddCommentCommandHandler : ICommandHandler<AddCommentCommand>
{
    private readonly IEventStore _eventStore;
    private readonly CommentProjection _commentProjection;

    public AddCommentCommandHandler(IEventStore eventStore, CommentProjection commentProjection)
    {
        _eventStore = eventStore;
        _commentProjection = commentProjection;
    }

    /// <summary>
    /// Handles the AddComment command by:
    /// 1. Validating the command
    /// 2. Creating a new comment aggregate
    /// 3. Executing the command on the aggregate (which generates events)
    /// 4. Persisting the events to the event store
    /// 5. Updating the comment projection
    /// </summary>
    public async Task HandleAsync(AddCommentCommand command)
    {
        // Step 1: Validate the command
        command.Validate();

        // Step 2: Create a new comment aggregate with a new ID
        var commentId = Guid.NewGuid();
        var aggregate = new CommentAggregate(commentId);

        // Step 3: Execute the command on the aggregate
        // This validates business rules and generates domain events
        aggregate.AddComment(command);

        // Step 4: Persist all uncommitted events to the event store
        var events = aggregate.GetUncommittedEvents();
        foreach (var @event in events)
        {
            await _eventStore.AppendEventAsync(@event);
        }

        // Mark events as committed
        aggregate.MarkEventsAsCommitted();

        // Step 5: Update the comment projection (read model)
        await _commentProjection.RebuildForEntityAsync(command.EntityType, command.EntityId);
    }
}
