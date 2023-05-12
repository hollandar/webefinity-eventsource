using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// An event that is passed to an event bus, that describes the event that has happened to an entity.
    /// Aggregate events are streamed (if enabled via an IEventStreamer service) after events have been applied to the data store.
    /// </summary>
    /// <typeparam name="TKey">The key type of the event.</typeparam>
    /// <typeparam name="TEventBase">The base of the event heirarchy for the aggregate.</typeparam>
    public sealed class AggregateRootEvent<TKey, TEventBase> : IAggregateRootEvent
    {
        /// <summary>
        /// Key of the aggregate that the event relates to
        /// </summary>
        public TKey Key { get; init; }

        /// <summary>
        /// An instance of the event as applied to the aggregate.
        /// </summary>
        public TEventBase Event { get; init; }


        /// <summary>
        /// Construct an aggregate root event
        /// </summary>
        /// <param name="key">The entity key.</param>
        /// <param name="event">The event applied to the entity.</param>
        public AggregateRootEvent(TKey key, TEventBase @event)
        {
            Key = key;
            Event = @event;
        }
    }
}
