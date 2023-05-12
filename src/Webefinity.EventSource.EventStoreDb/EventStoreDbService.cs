﻿using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Webefinity.EventSourcing.EventStoreDb.Options;
using Webefinity.EventSource.Core;
using System.Text;

namespace Webefinity.EventSource.EventStoreDb
{
    /// <summary>
    /// An event storage layer which stores events in an instance of EventStoreDb
    /// </summary>
    public class EventStoreDbService : IEventStoreService, IDisposable
    {
        private EventStoreClient eventStoreClient;
        private readonly ILogger<EventStoreDbService>? logger;

        /// <summary>
        /// Construct an event store service.
        /// </summary>
        /// <param name="options">Event store configuration options.</param>
        /// <param name="serviceProvider">DI service provider.</param>
        public EventStoreDbService(IOptions<EventStoreOptions> options, IServiceProvider serviceProvider)
        {
            var settings = new EventStoreClientSettings
            {
                ConnectivitySettings =
                {
                    Address = new Uri(options.Value.Uri)
                }
            };

            this.eventStoreClient = new EventStoreClient(settings);
            this.logger = serviceProvider.GetService<ILogger<EventStoreDbService>>();

            logger.FastLog(LogLevel.Information, "Storing events using EventStoreDb at {0}".LogFormat(options.Value.Uri));
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<EventContainer> ReadStreamAsync(string streamName)
        {
            var readStreamResult = this.eventStoreClient.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start);
            var readState = await readStreamResult.ReadState;
            if (readState == ReadState.Ok)
            {
                await foreach (var @event in readStreamResult)
                {
                    var jsonString = Encoding.UTF8.GetString(@event.Event.Data.ToArray());
                    yield return new EventContainer
                    (
                        @event.Event.EventType,
                        @event.Event.EventNumber.ToInt64(),
                        jsonString,
                        @event.Event.Created
                    );
                }
            }
        }

        /// <inheritdoc/>
        public async Task<long> AppendToStreamAsync(string stream, string eventType, long version, string json)
        {
            var jsonBinary = Encoding.UTF8.GetBytes(json);
            var eventData = new EventData(Uuid.NewUuid(), eventType, jsonBinary);

            IWriteResult writeResult;
            if (version == -1)
            {
                writeResult = await this.eventStoreClient.AppendToStreamAsync(stream, StreamState.NoStream, new List<EventData> { eventData });
            }
            else
            {
                writeResult = await this.eventStoreClient.AppendToStreamAsync(stream, StreamRevision.FromInt64(version - 1), new List<EventData> { eventData });
            }

            return writeResult.NextExpectedStreamRevision.ToInt64() + 1;
        }


        /// <summary>
        /// Dispose of the event store client when we are done.
        /// </summary>
        public void Dispose()
        {
            this.eventStoreClient.Dispose();
        }
    }
}
