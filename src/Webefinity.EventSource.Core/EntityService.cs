﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Reflection;

namespace Webefinity.EventSource.Core
{

    /// <inheritdoc/>
    public class EntityService : IEntityService
    {
        private IDictionary<string, MethodInfo> applyMethodInfoMemo = new Dictionary<string, MethodInfo>();
        private IEventStoreService eventStore;
        private IEntityCache? entityCache;
        private ILogger<EntityService>? logger;
        private IEventStreamer? eventStreamer;

        /// <summary>
        /// Construct an EntityService.
        /// Usually this is registered with dependency injection as a singleton.
        /// 
        /// The EntityService makes use of:
        /// 1. IEntityStoreService - required - A service like InMemoryDb or EventSourceDb capable of storing and retrieving events.
        /// 2. IEntityCache - optional - A caching layer that holds recently used entites so they are not rebuild every request.
        /// 3. IEntityStreamer - optional - A streaming interface that can push events onto an event bus once they are successfully applied to the event store.
        /// 4. ILogger - required - A standard logger.
        /// </summary>
        /// <param name="serviceProvider">The DI service provider.</param>
        public EntityService(IServiceProvider serviceProvider)
        {
            this.eventStore = serviceProvider.GetRequiredService<IEventStoreService>();
            this.entityCache = serviceProvider.GetService<IEntityCache>();
            this.eventStreamer = serviceProvider.GetService<IEventStreamer>();
            this.logger = serviceProvider.GetService<ILogger<EntityService>>();

            if (this.entityCache is not null)
            {
                logger.FastLog(LogLevel.Information, "Entity caching is active using provider {0}".LogFormat(() => this.entityCache.GetType().Name));
            }

        }



        /// <inheritdoc/>
        public async ValueTask<TEntity> GetEntity<TEntity, TEventBase>(Guid id) where TEntity : IAggregateRootWithGuidKey, IAggregateRoot<TEventBase>, new()
        {
            logger.FastLog(LogLevel.Debug, "Loading enity with id {0}".LogFormat(id));

            if (this.entityCache?.TryGetEntity(id, out var entity) ?? false)
            {
                logger.FastLog(LogLevel.Debug, "Satisfied from cache with id {0}.".LogFormat(id));
                return (TEntity)entity.Entity;
            }

            logger.FastLog(LogLevel.Debug, "Replaying events against {0}.".LogFormat(id));
            var start = Stopwatch.GetTimestamp();

            var guidEntity = new TEntity();
            guidEntity.Id = id;

            var version = await ReplayEvents<TEventBase>(guidEntity);

            logger.FastLog(LogLevel.Debug, "Took {0}ms".LogFormat(() => Stopwatch.GetElapsedTime(start).TotalMilliseconds));

            if (this.entityCache is not null)
            {
                this.entityCache.UpdateCache(guidEntity, version);
            }

            return guidEntity;
        }


        /// <inheritdoc/>
        public async ValueTask<TEntity> GetEntity<TEntity, TEventBase>(long id) where TEntity : IAggregateRootWithLongKey, IAggregateRoot<TEventBase>, new()
        {
            logger.FastLog(LogLevel.Debug, "Loading enity with id {0}".LogFormat(id));
            if (this.entityCache?.TryGetEntity(id, out var entity) ?? false)
            {
                logger.FastLog(LogLevel.Debug, "Satisfied from cache with id {0}.".LogFormat(id));
                return (TEntity)entity.Entity;
            }

            logger.FastLog(LogLevel.Debug, "Replaying events against {0}.".LogFormat(id));
            var start = Stopwatch.GetTimestamp();

            var longEntity = new TEntity();
            longEntity.Id = id;

            var version = await ReplayEvents<TEventBase>(longEntity);

            logger.FastLog(LogLevel.Debug, "Took {0}ms".LogFormat(() => Stopwatch.GetElapsedTime(start).TotalMilliseconds));

            if (this.entityCache is not null)
            {
                this.entityCache.UpdateCache(longEntity, version);
            }
            return longEntity;
        }

        /// <inheritdoc/>
        public async ValueTask<TEntity> GetEntity<TEntity, TEventBase>(string id) where TEntity : IAggregateRootWithStringKey, IAggregateRoot<TEventBase>, new()
        {
            logger.FastLog(LogLevel.Debug, "Loading enity with id {0}".LogFormat(id));
            if (this.entityCache?.TryGetEntity(id, out var entity) ?? false)
            {
                logger.FastLog(LogLevel.Debug, "Satisfied from cache with id {0}.".LogFormat(id));
                return (TEntity)entity.Entity;
            }

            logger.FastLog(LogLevel.Debug, "Replaying events against {0}.".LogFormat(id));
            var start = Stopwatch.GetTimestamp();

            var stringEntity = new TEntity();
            stringEntity.Id = id;

            long version = await ReplayEvents<TEventBase>(stringEntity);

            logger.FastLog(LogLevel.Debug, "Took {0}ms".LogFormat(() => Stopwatch.GetElapsedTime(start).TotalMilliseconds));

            if (this.entityCache is not null)
            {
                this.entityCache.UpdateCache(stringEntity, version);
            }

            return stringEntity;
        }

        /// <summary>
        /// Replay events against the aggregate from the underlying datastore.
        /// </summary>
        /// <typeparam name="TEventBase">Event base class.</typeparam>
        /// <param name="aggregateRoot">The aggregate root instance.</param>
        /// <returns></returns>
        protected async Task<long> ReplayEvents<TEventBase>(IAggregateRoot<TEventBase> aggregateRoot)
        {
            if (eventStore != null)
            {
                var aggregateRootType = aggregateRoot.GetType();
                object id = aggregateRoot.GetAggregateRootId();
                string name = aggregateRootType.Name;
                string streamName = $"{name}_{id}";
                long version = -1;
                await foreach (var container in this.eventStore.ReadStreamAsync(streamName))
                {
                    var serializer = aggregateRoot.Serializers.Where(r => r.EventType == container.EventType).FirstOrDefault();
                    if (serializer == null)
                    {
                        var exception = new EntityServiceException($"Could not find a serializer for {container.EventType}.");
                        logger.FastLogException(LogLevel.Error, exception);

                        throw exception;
                    }

                    logger.FastLog(LogLevel.Debug, "Replaying Event with timestamp {0} version {1} json {2}.".LogFormat(container.TimestampDto, container.Version, container.Json));
                    var eventObject = serializer.Deserialize(container.Json);
                    if (eventObject == null)
                    {
                        var exception = new EntityServiceException($"Could not deserialize {container.EventType} from {container.Json}.");
                        logger.FastLogException(LogLevel.Error, exception);

                        throw exception;
                    }

                    await this.ApplyEvent<TEventBase>(aggregateRoot, eventObject);

                    version = container.Version;
                }

                return version;
            }

            return 0;
        }


        /// <inheritdoc/>
        public async Task<IAggregateRoot<TEventBase>> Apply<TEventBase>(IAggregateRoot<TEventBase> aggregateRoot, IEnumerable<TEventBase> events)
        {
            var aggregateResult = aggregateRoot;
            foreach (var @event in events)
            {
                aggregateResult = await this.Apply(aggregateResult, @event);
            }

            return aggregateResult;
        }

        /// <summary>
        /// Apply a single event to an entity.
        /// </summary>
        /// <typeparam name="TEventBase">Event base class.</typeparam>
        /// <param name="aggregateRoot">The entity.</param>
        /// <param name="event">The event.</param>
        /// <returns>The adjusted entity.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected Task<IAggregateRoot<TEventBase>> ApplyEvent<TEventBase>(IAggregateRoot<TEventBase> aggregateRoot, TEventBase @event)
        {

            if (@event == null) throw new ArgumentNullException();

            var aggregateRootType = aggregateRoot.GetType();
            var eventType = @event.GetType();
            var key = aggregateRootType.Name + "/" + eventType.Name;
            MethodInfo? method;
            if (!applyMethodInfoMemo.TryGetValue(key, out method))
            {
                method = aggregateRootType.GetMethod("Apply", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    new Type[] { eventType });
                if (method is null)
                {
                    var exception = new InvalidOperationException($"{aggregateRootType.Name} does not contain a apply method for {eventType.Name}");
                    logger.FastLogException(LogLevel.Error, exception);

                    throw exception;
                };

                applyMethodInfoMemo[key] = method;
            }

            if (method != null)
            {
                method.Invoke(aggregateRoot, new object[] { @event });
            }

            return Task.FromResult(aggregateRoot);
        }

        /// <inheritdoc/>
        public async Task<IAggregateRoot<TEventBase>> Apply<TEventBase>(IAggregateRoot<TEventBase> aggregateRoot, TEventBase @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var aggregateRootType = aggregateRoot.GetType();
            var eventType = @event.GetType();
            long version = 0;
            if (eventStore != null)
            {
                // We have a serializer and serialization is enabled
                var serializer = aggregateRoot.Serializers.Where(r => r.CanSerialize(eventType)).FirstOrDefault();
                if (serializer == null)
                {
                    var exception = new EntityServiceException($"No serializer for {eventType.Name}");
                    logger.FastLogException(LogLevel.Error, exception);

                    throw exception;
                }

                object id = aggregateRoot.GetAggregateRootId();

                var typeName = aggregateRootType.Name;
                var streamName = $"{typeName}_{id}";
                if (this.entityCache?.TryGetEntityItem(aggregateRoot, out var entityItem) ?? false)
                {
                    // We have loaded this entity already;
                    version = entityItem.Version + 1;
                }
                else
                {
                    logger.FastLog(LogLevel.Debug, "Satisfying version lookup for {0} from cache.".LogFormat(streamName));

                    // We havent loaded it, find the next version;
                    await foreach (var container in this.eventStore.ReadStreamAsync(streamName))
                    {
                        version = container.Version + 1;
                    }
                }

                var eventJson = serializer.Serialize(@event);
                logger.FastLog(LogLevel.Debug, "Storing {0} version {1} as {2}".LogFormat(serializer.EventType, version, eventJson));

                await this.eventStore.AppendToStreamAsync(streamName, serializer.EventType, version, eventJson);
            }


            var ent = await ApplyEvent(aggregateRoot, @event);
            if (entityCache is not null)
            {
                logger.FastLog(LogLevel.Debug, "Updating entity cache with {0}".LogFormat(version));
                entityCache.UpdateCache(ent, version);
            }

            if (eventStreamer is not null)
            {
                logger.FastLog(LogLevel.Debug, "Streaming event {0}.".LogFormat(() => @event.GetType().Name));
                IAggregateRootEvent eventContainer = aggregateRoot switch
                {
                    IAggregateRootWithGuidKey guidRoot => new AggregateRootEvent<Guid, TEventBase>(guidRoot.Id, @event),
                    IAggregateRootWithLongKey guidRoot => new AggregateRootEvent<Int64, TEventBase>(guidRoot.Id, @event),
                    IAggregateRootWithStringKey guidRoot => new AggregateRootEvent<string, TEventBase>(guidRoot.Id, @event),
                    _ => throw new UnreachableException()
                };

                await eventStreamer.StreamEventAsync(eventContainer);
            }

            return ent;
        }
    }
}