using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// A base marker interface for all aggregate roots.
    /// </summary>
    public interface IAggregateRoot { }

    /// <summary>
    /// The specialized interface for aggregate roots which marks the type of the events that can be serialized or deserialized.
    /// </summary>
    /// <typeparam name="TEventBase">TEventBase is generally a base class from which all events derive.</typeparam>
    public interface IAggregateRoot<TEventBase> : IAggregateRoot
    {
        /// <summary>
        /// A list of serializers that are associated with the aggregate root.
        /// A serializer maps an AggregateRoot entity and an Event to an event name for storage, and identifies the possible events for an aggregate.
        /// 
        /// While this cant be an abstract static, you should be careful to instantiate the serializers only once.
        /// Either encapsulate them in a private Management class or just delegate this instance variable to a static collection.
        /// 
        /// Either:
        /// static List<EventSerializer<YourEventBase>> serializers = new(){};
        /// public List<EventSerializer<YourEventBase>> Serializers => serializers;
        /// 
        /// Or, to allow set up of serialization options:
        /// class SerializerManager {
        ///     public static SerializerManager Instance => new SerializerManager();
        ///     public List<EventSerializer<YourEventBase>> serializers;
        ///     SerializerManager() {
        ///         this.serializers = new () {};
        ///     }
        /// }
        /// public List<EventSerializer<YourSerializerBase>> Serializers => SerializerManager.instance.serializers;
        /// </summary>
        abstract List<EventSerializer<TEventBase>> Serializers { get; }
    }
}
