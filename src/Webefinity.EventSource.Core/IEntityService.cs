using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// The entity service is responsible for loading and creating entities.
    /// It is also responsible for applying events to entities, coordinating entity caching (optional), entity storage and event streaming (optional).
    /// </summary>
    public interface IEntityService
    {
        /// <summary>
        /// Return a rebuilt entity with a given Guid id.
        /// The entity may be rebuilt on the fly (which is very fast) or from an attached entity cache.
        /// To create an entity, simply GetEntity with a new id.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <typeparam name="TEventBase">Entity event type.</typeparam>
        /// <param name="id">Entity id.</param>
        /// <returns>The entity.</returns>
        ValueTask<TEntity> GetEntity<TEntity, TEventBase>(Guid id) where TEntity : IAggregateRootWithGuidKey, IAggregateRoot<TEventBase>, new();

        /// <summary>
        /// Return a rebuilt entity with a given long id.
        /// The entity may be rebuilt on the fly (which is very fast) or from an attached entity cache.
        /// To create an entity, simply GetEntity with a new id.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <typeparam name="TEventBase">Entity event type.</typeparam>
        /// <param name="id">Entity id.</param>
        /// <returns>The entity.</returns>
        ValueTask<TEntity> GetEntity<TEntity, TEventBase>(long id) where TEntity : IAggregateRootWithLongKey, IAggregateRoot<TEventBase>, new();

        /// <summary>
        /// Return a rebuilt entity with a given string id.
        /// The entity may be rebuilt on the fly (which is very fast) or from an attached entity cache.
        /// To create an entity, simply GetEntity with a new id.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <typeparam name="TEventBase">Entity event type.</typeparam>
        /// <param name="id">Entity id.</param>
        /// <returns>The entity.</returns>
        ValueTask<TEntity> GetEntity<TEntity, TEventBase>(string id) where TEntity : IAggregateRootWithStringKey, IAggregateRoot<TEventBase>, new();

        /// <summary>
        /// Apply an entity to an aggregate root.
        /// The event must be of the correct type, and registered in the Serializers list for the aggregate root object.
        /// </summary>
        /// <typeparam name="TEventBase">The base class of events.</typeparam>
        /// <param name="aggregateRoot">The entity.</param>
        /// <param name="events">Event(s) to be applied.</param>
        /// <returns>The adjusted entity.</returns>
        Task<IAggregateRoot<TEventBase>> Apply<TEventBase>(IAggregateRoot<TEventBase> aggregateRoot, IEnumerable<TEventBase> events);

        /// <summary>
        /// Apply a single event to an entity.
        /// </summary>
        /// <typeparam name="TEventBase">Event base class.</typeparam>
        /// <param name="aggregateRoot">The entity.</param>
        /// <param name="event">The event to apply.</param>
        /// <returns>The entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown if event passed is null.</exception>
        /// <exception cref="UnreachableException">Thrown if the key type is not available for event streaming.</exception>
        Task<IAggregateRoot<TEventBase>> Apply<TEventBase>(IAggregateRoot<TEventBase> aggregateRoot, TEventBase @event);
    }
}
