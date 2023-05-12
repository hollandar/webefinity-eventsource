using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Core
{

    /// <summary>
    /// Extensions related to event sourcing.
    /// </summary>
    public static class EventSourceExtensions
    {
        /// <summary>
        /// Given an aggregate root, return its key as an object regardless of the key type.
        /// </summary>
        /// <param name="aggregateRoot">The entity.</param>
        /// <returns>The key value as object.</returns>
        /// <exception cref="UnreachableException">Thrown if the key type of the entity isnt defined.</exception>
        public static object GetAggregateRootId(this IAggregateRoot aggregateRoot)
        {
            object id = aggregateRoot switch
            {
                IAggregateRootWithGuidKey guidEntity => guidEntity.Id,
                IAggregateRootWithLongKey longEntity => longEntity.Id,
                IAggregateRootWithStringKey stringEntity => stringEntity.Id,
                _ => throw new UnreachableException($"{aggregateRoot.GetType().Name} does not have a key type."),
            };

            return id;
        }

        /// <summary>
        /// Get the aggregate id of an aggregate root, corecing it into the right type.
        /// </summary>
        /// <typeparam name="TId">Type of key you expect.</typeparam>
        /// <param name="aggregateRoot">The entity.</param>
        /// <returns>Strongly typed key, or null if the type was incorrect.</returns>
        public static TId? GetAggregateRootId<TId>(this IAggregateRoot aggregateRoot)
        {
            var id = aggregateRoot.GetAggregateRootId();
            if (id is TId)
            {
                return (TId)id;
            }
            
            return default(TId);
        }
    }
}
