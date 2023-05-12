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
        /// </summary>
        abstract List<EventSerializer<TEventBase>> Serializers { get; }
    }
}
