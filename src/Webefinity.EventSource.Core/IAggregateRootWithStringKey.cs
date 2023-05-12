using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// A marker interface for aggregate roots with strings as keys.
    /// </summary>
    public interface IAggregateRootWithStringKey
    {
        /// <summary>
        /// The Id of the entity.
        /// </summary>
        string Id { get; set; }
    }
}
