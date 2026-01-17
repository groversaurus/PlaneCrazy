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
        /// <typeparam name="TEvent">The type of event to append.</typeparam>
        /// <param name="streamId">The identifier of the event stream. Cannot be null or empty.</param>
        /// <param name="event">The event to append. Cannot be null.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when streamId or event is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when streamId is empty.</exception>
        Task AppendEventAsync<TEvent>(string streamId, TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;

        /// <summary>
        /// Reads events from a specific event stream.
        /// </summary>
        /// <typeparam name="TEvent">The type of events to read.</typeparam>
        /// <param name="streamId">The identifier of the event stream. Cannot be null or empty.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation that returns the events.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when streamId is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when streamId is empty.</exception>
        Task<IEnumerable<TEvent>> ReadEventsAsync<TEvent>(string streamId, CancellationToken cancellationToken = default) where TEvent : class;

        /// <summary>
        /// Reads all events from the event store.
        /// </summary>
        /// <typeparam name="TEvent">The type of events to read.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation that returns all events.</returns>
        Task<IEnumerable<TEvent>> ReadAllEventsAsync<TEvent>(CancellationToken cancellationToken = default) where TEvent : class;
    }
}
