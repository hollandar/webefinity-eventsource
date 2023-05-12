using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Cache.Memory.Options
{
    /// <summary>
    /// Options related in in memory caching.
    /// </summary>
    public class EntityCacheMemoryOptions
    {
        /// <summary>
        /// The period for which the entries in the cacge are valid, in minutes.
        /// </summary>
        public int CachePeriodMinutes { get; set; } = 30;
    }
}
