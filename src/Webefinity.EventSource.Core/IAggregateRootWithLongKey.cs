using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// A marker interface for aggregate roots with longs as keys.
    /// </summary>
    public interface IAggregateRootWithLongKey
    {
        /// <summary>
        /// The Id of the entity.
        /// </summary>
        long Id { get; set; }
    }
}
