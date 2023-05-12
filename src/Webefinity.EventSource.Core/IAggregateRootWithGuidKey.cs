using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// A marker interface for aggregate roots with Guids as keys.
    /// </summary>
    public interface IAggregateRootWithGuidKey
    {
        /// <summary>
        /// The Id of the entity.
        /// </summary>
        Guid Id { get; set; }
    }
}
