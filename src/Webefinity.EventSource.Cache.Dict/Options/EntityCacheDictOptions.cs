using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Cache.Dict.Options
{
    /// <summary>
    /// Options related to dictionary cache.
    /// </summary>
    public class EntityCacheDictOptions
    {
        /// <summary>
        /// Expire entries after how many minutes?
        /// Not that EvacuateCache(false) must be called to expire old entries.
        /// </summary>
        public int CachePeriodMinutes { get; set; } = 30;
    }
}
