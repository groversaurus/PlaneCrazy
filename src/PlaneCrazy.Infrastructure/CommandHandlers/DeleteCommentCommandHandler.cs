using Microsoft.Extensions.Logging;
using PlaneCrazy.Domain.Aggregates;
using PlaneCrazy.Domain.Commands;
using PlaneCrazy.Domain.Events;
using PlaneCrazy.Domain.Interfaces;
using PlaneCrazy.Infrastructure.Projections;

namespace PlaneCrazy.Infrastructure.CommandHandlers;

/// <summary>
/// Command handler for deleting comments.
/// Loads event stream, rebuilds aggregate state, validates command, emits events, and updates projections.
/// </summary>
public class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand>
{
    private readonly IEventStore _eventStore;
    private readonly CommentProjection _commentProjection;
    private readonly ILogger<DeleteCommentCommandHandler>? _logger;

    public DeleteCommentCommandHandler(
        IEventStore eventStore, 
        CommentProjection commentProjection,
        ILogger<DeleteCommentCommandHandler>? logger = null)
    {
        _eventStore = eventStore;
        _commentProjection = commentProjection;
        _logger = logger;
    }

    /// <summary>
    /// Handles the DeleteComment command by:
    /// 1. Validating the command
    /// 2. Loading the comment's event stream from the event store
    /// 3. Rebuilding the comment aggregate state from historical events
    /// 4. Executing the command on the aggregate (which validates business rules and generates new events)
    /// 5. Persisting the new events to the event store
    /// 6. Updating the comment projection
    /// </summary>
    public async Task HandleAsync(DeleteCommentCommand command)
    {
        _logger?.LogInformation("Handling DeleteComment command for comment {CommentId}", 
            command.CommentId);
        
        try
        {
            // Step 1: Validate the command
            command.Validate();
            _logger?.LogDebug("Command validation passed");

            // Step 2: Load all events for this comment from the event store
            // Filter events by comment ID
            var allEvents = await _eventStore.ReadAllEventsAsync();
            var commentEvents = allEvents
                .Where(e => IsCommentEvent(e, command.CommentId))
                .OrderBy(e => e.OccurredAt)
                .ToList();

            // Step 3: Rebuild the aggregate state from the event stream
            var aggregate = new CommentAggregate(command.CommentId);
            aggregate.LoadFromHistory(commentEvents);

            // Step 4: Execute the command on the aggregate
            // This validates business rules and generates new domain events
            aggregate.DeleteComment(command);

            // Step 5: Persist all uncommitted events to the event store
            var events = aggregate.GetUncommittedEvents();
            _logger?.LogDebug("Generated {EventCount} events", events.Count());
            
            foreach (var @event in events)
            {
                await _eventStore.AppendEventAsync(@event);
                _logger?.LogDebug("Persisted event {EventType}", @event.EventType);
            }

            // Mark events as committed
            aggregate.MarkEventsAsCommitted();

            // Step 6: Update the comment projection (read model)
            _logger?.LogDebug("Updating CommentProjection for {EntityType}:{EntityId}", 
                command.EntityType, command.EntityId);
            await _commentProjection.RebuildForEntityAsync(command.EntityType, command.EntityId);
            
            _logger?.LogInformation("Successfully handled DeleteComment command for comment {CommentId}", 
                command.CommentId);
        }
        catch (ArgumentException ex)
        {
            _logger?.LogWarning(ex, "Validation failed for DeleteComment command");
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling DeleteComment command for comment {CommentId}", 
                command.CommentId);
            throw;
        }
    }

    /// <summary>
    /// Determines if an event is related to a specific comment.
    /// </summary>
    private bool IsCommentEvent(DomainEvent @event, Guid commentId)
    {
        return @event switch
        {
            CommentAdded added => added.CommentId == commentId,
            CommentEdited edited => edited.CommentId == commentId,
            CommentDeleted deleted => deleted.CommentId == commentId,
            _ => false
        };
    }
}
