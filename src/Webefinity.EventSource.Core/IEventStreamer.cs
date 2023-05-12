using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// An interface that allows events to be streamed out of the event store after they are applied.
    /// </summary>
    public interface IEventStreamer
    {
        /// <summary>
        /// Stream an event onto the event bus, however that is implemented.
        /// </summary>
        /// <param name="evt">An AggregateRootEvent to stream.</param>
        /// <returns>Awaitable task.</returns>
        Task StreamEventAsync (IAggregateRootEvent evt);
    }
}
