using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Webefinity.EventSource.Cache.Memory.Options;
using Webefinity.EventSource.Core;
using System.Runtime.ExceptionServices;

namespace Webefinity.EventSource.Cache.Memory
{
    /// <summary>
    /// An entity cache that makes use of the IMemoryCache service.
    /// Entities will be automatically evaculated after the expiry elapses.
    /// </summary>
    public sealed class EntityCacheMemoryService : IEntityCache
    {
        private CancellationTokenSource expiryCancellationToken = new CancellationTokenSource();
        private readonly IMemoryCache memoryCache;
        private readonly IOptions<EntityCacheMemoryOptions> options;
        private readonly ILogger<EntityCacheMemoryService>? logger;

        /// <summary>
        /// Construct an memory based entity cache.
        /// Expects:
        /// 1. IMemoryCache - required - the underlying in memory store.
        /// 2. IOptions<EntityCacheMemoryOptions> - required - options related to in memory storage.
        /// </summary>
        /// <param name="serviceProvider">DI service provider</param>
        public EntityCacheMemoryService(IServiceProvider serviceProvider)
        {
            this.memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            this.options = serviceProvider.GetRequiredService<IOptions<EntityCacheMemoryOptions>>();
            this.logger = serviceProvider.GetService<ILogger<EntityCacheMemoryService>>();
            logger.FastLog(LogLevel.Information, "Caching entities using in memory caching for cache period {} min.".LogFormat(this.options.Value.CachePeriodMinutes));

        }

        /// <inheritdoc/>
        /// <param name="force">True, false has no meaning as entries expire automatically.</param>
        public void EvacuateCache(bool force = false)
        {
            if (force)
            {
                if (!expiryCancellationToken.IsCancellationRequested && expiryCancellationToken.Token.CanBeCanceled)
                {
                    expiryCancellationToken.Cancel();
                    expiryCancellationToken.Dispose();
                }

                expiryCancellationToken = new CancellationTokenSource();
            }
        }


        /// <inheritdoc/>
        public bool TryGetEntityInternal(object id, out EntityItem entity)
        {
            entity = EntityItem.Empty;
            if (this.memoryCache.TryGetValue<EntityItem>(id, out var cacheEntry))
            {
                if (cacheEntry is not null)
                {
                    entity = cacheEntry;
                    return true;
                }
                return false;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool TryGetEntity(Guid id, out EntityItem entity)
        {
            return TryGetEntityInternal(id, out entity);
        }

        /// <inheritdoc/>
        public bool TryGetEntity(long id, out EntityItem entity)
        {
            return TryGetEntityInternal(id, out entity);
        }

        /// <inheritdoc/>
        public bool TryGetEntity(string id, out EntityItem entity)
        {
            return TryGetEntityInternal(id, out entity);
        }

        /// <inheritdoc/>
        public bool TryGetEntityItem(IAggregateRoot aggregateRoot, out EntityItem entity)
        {
            entity = EntityItem.Empty;
            var key = aggregateRoot.GetAggregateRootId();
            return TryGetEntityInternal(key, out entity);
        }


        /// <inheritdoc/>
        public void UpdateCache(IAggregateRoot aggregateRoot, long version)
        {
            var memoryOptions = new MemoryCacheEntryOptions();
            memoryOptions.SetPriority(CacheItemPriority.Normal);
            memoryOptions.AddExpirationToken(new CancellationChangeToken(expiryCancellationToken.Token));
            memoryOptions.SetAbsoluteExpiration(
                DateTimeOffset.UtcNow.AddMinutes(this.options.Value.CachePeriodMinutes)
            );
            var key = aggregateRoot.GetAggregateRootId();
            var entityItem = new EntityItem(aggregateRoot, version, memoryOptions.AbsoluteExpiration);

            memoryCache.Set(key, entityItem, memoryOptions);
        }
    }
}