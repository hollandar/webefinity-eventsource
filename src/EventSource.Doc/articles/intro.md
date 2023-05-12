# Webfinity Event Store Entity Layer

EventStore is a storage layer for entities, where each entity is internally stored and rebuilt by replaying a sequence of events in order.

Colloquially this is known as Event Sourcing.

EventStore provides in memory and file based stores, but is primarily intended for use with an event store.  A connector thats stores events in [EventStoreDb](https://www.eventstore.com/eventstoredb) exists, and the platform could easily be extended to support other event storage structures.

Event Store supports entity keys that are Guids, strings or longs.

Entity classes are plain old .NET classes as are the events that get applied to them.
In general the lifecycle of an entity is creation, then update through a series of event changes through time.  You dont modify the entity directly, rather you update it by applying new events to it that change its data.

Reloading an entity involves replaying all the events for the entity against it in time sequence, a process which is faster than you expect.

## Getting Started

In most applications you will use the EventStoreDb storage provider with in memory caching.

``` powershell
dotnet package add Webefinity.EventSource.EventStoreDb
dotnet package add Webefinity.EventSource.Cache.Memory
```

## Getting Started in Development

For development, you might choose a file based provider with dictionary caching

``` powershell
dotnet package add Webefinity.EventSource.InMemoryDb
dotnet package add Webefinity.EventSource.Cache.Dict
```

In this mode, remember that entities will not be retained if you restart the app.
If you want that to happen, either install EventStoreDb using Docker, or use the file based event store.
