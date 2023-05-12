using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// An abstract base for event serializers.
    /// You may, but would not usually implement your own serializer.
    /// </summary>
    /// <typeparam name="TEventBase"></typeparam>
    public abstract class EventSerializer<TEventBase>
    {
        /// <summary>
        /// Deserialize a json event.
        /// </summary>
        /// <param name="json">The event.</param>
        /// <returns>An instance of TEventBase deseralized.</returns>
        public abstract TEventBase? Deserialize(string json);

        /// <summary>
        /// Serialize a json event.
        /// </summary>
        /// <param name="event">The TEventBase instance to serialize.</param>
        /// <returns>The serialized version of the event as json.</returns>
        public abstract string Serialize(TEventBase @event);

        /// <summary>
        /// The name of the event type as stored in the event store.
        /// </summary>
        public abstract string EventType { get; }

        /// <summary>
        /// Determine if an instance of the given type can be serialized?
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>True if the type can be serialized.</returns>
        public abstract bool CanSerialize<T>();

        /// <summary>
        /// Determine if an instance of the given type can be serialized?
        /// </summary>
        /// <param name="type">The type.</typeparam>
        /// <returns>True if the type can be serialized.</returns>
        public abstract bool CanSerialize(Type type);

        /// <summary>
        /// Get the System.Type of the event being stored.
        /// </summary>
        /// <returns>The type.</returns>
        public abstract Type GetEventType();
    }

    /// <summary>
    /// The core event serializer.
    /// This serializer uses System.Text.Json.JsonSerializer to serialize and deserialize event objects.
    /// </summary>
    /// <typeparam name="TEventBase"></typeparam>
    /// <typeparam name="TEventType"></typeparam>
    public class EventSerializer<TEventBase, TEventType> : EventSerializer<TEventBase> where TEventType : TEventBase
    {
        private string eventType;

        /// <summary>
        /// Construct an event serializer.
        /// </summary>
        /// <param name="eventType">The name to be used when serializing and deserializing the event.</param>
        public EventSerializer(string? eventType = null)
        {
            this.eventType = eventType ?? typeof(TEventType).Name;
        }

        /// <inheritdoc/>
        public override string EventType => eventType;

        /// <inheritdoc/>
        public override TEventBase? Deserialize(string json)
        {
            return JsonSerializer.Deserialize<TEventType>(json);
        }

        /// <inheritdoc/>
        public override string Serialize(TEventBase @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException();
            }

            var eventType = (TEventType)@event;
            return JsonSerializer.Serialize<TEventType>((TEventType)@event);
        }

        /// <inheritdoc/>
        public override bool CanSerialize<T>()
        {
            return CanSerialize(typeof(T));
        }

        /// <inheritdoc/>
        public override bool CanSerialize(Type type)
        {
            return type == typeof(TEventType);
        }

        /// <inheritdoc/>
        public override Type GetEventType()
        {
            return typeof(TEventType);
        }
    }
}
