using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// The storage container that holds the json of the event in the event store.
    /// </summary>
    public class EventContainer
    {
        private string eventType;
        private long version;
        private string json;
        private DateTimeOffset timestamp;

        /// <summary>
        /// Construct an event container.
        /// </summary>
        /// <param name="eventType">The name of the event to store.  (Not necessarily its reflected type name depending upon your serializer)</param>
        /// <param name="version">The version that this event creates.</param>
        /// <param name="json">The serialized JSON of the event.</param>
        /// <param name="timestamp">The time of the event.</param>
        public EventContainer(string eventType, long version, string json, DateTimeOffset timestamp)
        {
            this.eventType = eventType;
            this.version = version;
            this.json = json;
            this.timestamp = timestamp;
        }

        /// <summary>
        /// The type of event.
        /// This is a text string may not be the type name of the event class.
        /// </summary>
        public string EventType { get { return eventType; } }

        /// <summary>
        /// The version of the entity that this events creates.
        /// </summary>
        public long Version { get { return version; } }

        /// <summary>
        /// Serialized JSON of the event
        /// </summary>
        public string Json { get { return json; } }

        /// <summary>
        /// Time of the event in Unix Milliseconds.
        /// </summary>
        public long Timestamp { get { return timestamp.ToUnixTimeMilliseconds(); } }

        /// <summary>
        /// Timestamp of the event as a DateTimeOffset.
        /// </summary>
        public DateTimeOffset TimestampDto { get { return timestamp; } }

    }

    /// <summary>
    /// Interface for an event storage service
    /// </summary>
    public interface IEventStoreService
    {
        /// <summary>
        /// Read events from a stream.
        /// </summary>
        /// <param name="streamName">Name of the stream to read.</param>
        /// <returns></returns>
        IAsyncEnumerable<EventContainer> ReadStreamAsync(string streamName);

        /// <summary>
        /// Append events to a stream.
        /// </summary>
        /// <param name="stream">Name of the stream to write</param>
        /// <param name="eventType">Event type to write.</param>
        /// <param name="version">Version to write.</param>
        /// <param name="json">JSON of the serialized event.</param>
        /// <returns></returns>
        Task<long> AppendToStreamAsync(string stream, string eventType, long version, string json);
    }
}
