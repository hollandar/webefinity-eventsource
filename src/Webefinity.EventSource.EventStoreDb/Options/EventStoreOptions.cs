using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSourcing.EventStoreDb.Options
{
    /// <summary>
    /// Options related to EventStoreDb
    /// </summary>
    public class EventStoreOptions
    {
        /// <summary>
        /// The URL of the event store database, defaults to localhost.
        /// </summary>
        public string Uri { get; set; } = "esdb+discover://localhost:2113?tls=false&keepAliveTimeout=10000&keepAliveInterval=10000";
    }
}
