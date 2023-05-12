# Event Streaming

EventStore can pass your events on to an event bus, or post process events as you require in a streamer.  There are no preimplemented streamer implementations.

Implementing a streamer is just a matter of implementing the IEventStreamer interface on a service and registering it as the IEventStreamer:

``` c#

public class MyEventStreamerService : IEventStreamer {

    public Task StreamEventAsync(IAggregateRootEvent evt) {
        var json = JsonSerializer.Serialize(evt);
        Console.WriteLine($"New event: {json}");

        return Task.CompletedTask;
    }
}

serviceCollection.AddSingleton<IEventStreamer, MyEventStreamerService>();

```

The evt object is an instance of AggregateRootEvent for the key type and event base that you used, so you can do further introspection on the evt object to handle different events in different ways, filter them or wrap them as you require, before you stream them out.

Events are not received by the streamer until after they have successfully been stored.

As streamers are awaited, they should delegate their work to a thread or hand the event off to another process.  Do not do long running calculation in the streamer itself, that will slow the storage of every new event.