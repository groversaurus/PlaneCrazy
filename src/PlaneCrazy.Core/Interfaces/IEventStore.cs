using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlaneCrazy.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for an event store that supports appending and reading events.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        /// Appends an event to the event store.
        /// </summary>
        /// <param name="streamId">The identifier of the event stream.</param>
        /// <param name="event">The event to append.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AppendEventAsync(string streamId, object @event, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads events from a specific event stream.
        /// </summary>
        /// <param name="streamId">The identifier of the event stream.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation that returns the events.</returns>
        Task<IEnumerable<object>> ReadEventsAsync(string streamId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads all events from the event store.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation that returns all events.</returns>
        Task<IEnumerable<object>> ReadAllEventsAsync(CancellationToken cancellationToken = default);
    }
}
