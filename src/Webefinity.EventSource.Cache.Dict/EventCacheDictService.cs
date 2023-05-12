using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Webefinity.EventSource.Cache.Dict.Options;
using Webefinity.EventSource.Core;

namespace Webefinity.EventSource.Cache.Dict
{
    ///<inheritdoc/>
    public sealed class EntityCacheDictService : IEntityCache
    {
        private readonly IOptions<EntityCacheDictOptions> options;
        private readonly ILogger<EntityCacheDictService>? logger;
        private readonly Dictionary<object, EntityItem> _cache = new();
        private readonly SemaphoreSlim semaphore = new(1);

        /// <summary>
        /// Construct an entity cache backed by a dictionary.
        /// Useful for testing, but Cache.Memory is preferred for production.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="serviceProvider">DI service provider.</param>
        public EntityCacheDictService(IOptions<EntityCacheDictOptions> options, IServiceProvider serviceProvider)
        {
            this.options = options;
            this.logger = serviceProvider.GetService<ILogger<EntityCacheDictService>>();
            logger.FastLog(LogLevel.Information, "Caching entities using an in memory dictionary.".LogFormat());
        }


        /// <inheritdoc/>
        /// <param name="force">True to clear the cache, False to evaculated expires entries.</param>
        public void EvacuateCache(bool force = false)
        {
            try
            {
                semaphore.Wait();
                if (force)
                {
                    _cache.Clear();
                }
                else
                {
                    var validUntil = DateTimeOffset.UtcNow;
                    foreach (var key in _cache.Keys)
                    {
                        var value = _cache[key];
                        if (validUntil > value.Expires)
                        {
                            _cache.Remove(key);
                        }
                    }
                }

                logger.FastLog(LogLevel.Debug, "Entity cache evacuated.".LogFormat());
            }
            finally
            {
                semaphore.Release();
            }
        }


        ///<inheritdoc/>
        public bool TryGetEntity(Guid id, out EntityItem entity)
        {
            if (_cache.TryGetValue(id, out var entityItem))
            {
                entity = entityItem;
                return true;
            }

            logger.FastLog(LogLevel.Debug, "Cache miss on id {}.".LogFormat(id));
            entity = EntityItem.Empty;
            return false;
        }

        ///<inheritdoc/>
        public bool TryGetEntity(long id, out EntityItem entity)
        {
            if (_cache.TryGetValue(id, out var entityItem))
            {
                entity = entityItem;
                return true;
            }

            logger.FastLog(LogLevel.Debug, "Cache miss on id {}.".LogFormat(id));
            entity = EntityItem.Empty;
            return false;
        }

        ///<inheritdoc/>
        public bool TryGetEntity(string id, out EntityItem entity)
        {
            if (_cache.TryGetValue(id, out var entityItem))
            {
                entity = entityItem;
                return true;
            }

            logger.FastLog(LogLevel.Debug, "Cache miss on id {}.".LogFormat(id));
            entity = EntityItem.Empty;
            return false;
        }

        ///<inheritdoc/>
        public bool TryGetEntityItem(IAggregateRoot aggregateRoot, out EntityItem entity)
        {
            var id = aggregateRoot.GetAggregateRootId();
            if (_cache.TryGetValue(id, out var entityItem))
            {
                entity = entityItem;
                return true;
            }

            logger.FastLog(LogLevel.Debug, "Cache miss on id {}.".LogFormat(id));
            entity = EntityItem.Empty;
            return false;
        }

        ///<inheritdoc/>
        public void UpdateCache(IAggregateRoot aggregateRoot, long version)
        {
            var id = aggregateRoot.GetAggregateRootId();
            _cache[id] = new EntityItem(aggregateRoot, version, DateTimeOffset.UtcNow.AddMinutes(this.options.Value.CachePeriodMinutes));
        }
    }
}
