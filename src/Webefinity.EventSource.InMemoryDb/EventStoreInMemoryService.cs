﻿using Webefinity.EventSource.Core;
using System.Diagnostics;

namespace Webefinity.EventSource.InMemoryDb
{
    /// <summary>
    /// An event store that holds events in memory only.
    /// Events are stored in a dictionary keyed on stream name.
    /// Clearly dont use this in production.
    /// </summary>
    public class EventStoreInMemoryService : IEventStoreService
    {
        IDictionary<string, IList<EventContainer>> events;

        /// <summary>
        /// Construct an in memory store.
        /// </summary>
        public EventStoreInMemoryService()
        {
            this.events = new Dictionary<string, IList<EventContainer>>();
        }

        /// <inheritdoc/>
        public Task<long> AppendToStreamAsync(string stream, string eventType, long version, string json)
        {
            IList<EventContainer>? eventList;
            if (!this.events.TryGetValue(stream, out eventList))
            {
                eventList = new List<EventContainer>();
                this.events[stream] = eventList;
            }

            eventList.Add(new EventContainer(eventType, version, json, DateTimeOffset.UtcNow));

            Debug.Assert(version == eventList.Count - 1);
            return Task.FromResult(version);
        }

#pragma warning disable CS1998

        /// <inheritdoc/>
        public async IAsyncEnumerable<EventContainer> ReadStreamAsync(string streamName)
        {
            if (this.events.TryGetValue(streamName, out var eventList))
            {
                foreach (var eventContainer in eventList) yield return eventContainer;
            }
        }

#pragma warning restore CS1998

    }
}