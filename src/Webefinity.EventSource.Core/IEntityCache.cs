using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// The EntityItem is the container for storage of cached entities.
    /// It tracks the entity version and when it expires from the cache.
    /// </summary>
    public class EntityItem
    {
        readonly IAggregateRoot? aggregateRoot;

        /// <summary>
        /// Construct a new cache item, containing the entity, its version and when it expires.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="version">Its version.</param>
        /// <param name="expires">When it expires.</param>
        public EntityItem(IAggregateRoot? entity, long version, DateTimeOffset? expires = null)
        {
            aggregateRoot = entity;
            Expires = expires ?? DateTimeOffset.UtcNow.AddMinutes(30);
            Version = version;
        }

        /// <summary>
        /// The instance of the entity held by the cache.
        /// </summary>
        public IAggregateRoot Entity
        {
            get
            {
                if (aggregateRoot is null)
                {
                    throw new NullReferenceException(nameof(Entity));
                }

                if (Expires < DateTimeOffset.UtcNow)
                {
                    throw new EntityExpiredException();
                }

                return aggregateRoot;
            }
        }

        /// <summary>
        /// Absolute expiry of the entity in Utc.
        /// </summary>
        public DateTimeOffset Expires { get; set; }

        /// <summary>
        /// The version of the entity stored.
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// An empty item.
        /// </summary>
        public static EntityItem Empty = new EntityItem(null, -1, DateTimeOffset.MinValue);
    }

    /// <summary>
    /// The generic definition of an entity cache.
    /// </summary>
    public interface IEntityCache
    {
        /// <summary>
        /// Try to get a cache item for the given aggregate root.
        /// </summary>
        /// <param name="aggregateRoot">The entity.</param>
        /// <param name="entity">The entity item.</param>
        /// <returns>True if the item was cached.</returns>
        bool TryGetEntityItem(IAggregateRoot aggregateRoot, out EntityItem entity);

        /// <summary>
        /// Try to get an entity by its Guid Id from the cache.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>True if the entity was cached.</returns>
        bool TryGetEntity(Guid id, out EntityItem entity);

        /// <summary>
        /// Try to get an entity by its long Id from the cache.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>True if the entity was cached.</returns>
        bool TryGetEntity(long id, out EntityItem entity);

        /// <summary>
        /// Try to get an entity by its string Id from the cache.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>True if the entity was cached.</returns>

        bool TryGetEntity(string id, out EntityItem entity);

        /// <summary>
        /// Clear the cache of entries.
        /// Force behaviour varies by implementation.
        /// </summary>
        /// <param name="force">True to force the evaculation.  See implementations for actual behaviour.</param>
        void EvacuateCache(bool force = false);

        /// <summary>
        /// Place a new version into the cache, in place of any existing version.
        /// </summary>
        /// <param name="aggregateRoot">The entity.</param>
        /// <param name="version">The version.</param>
        void UpdateCache(IAggregateRoot aggregateRoot, long version);
    }
}
