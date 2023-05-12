# Dependency Injection

Establishing the entity layer is a matter of injecting the services that your structure will require.

For a production structure using EventStoreDb:

``` c#
serviceCollection.AddLogging(options => {
    options.AddConsole();
});
serviceCollection.Configure<EventStoreOptions>(configuration.GetSection("EventStoreDb"));
serviceCollection.Configure<EntityCacheMemoryOptions>(configuration.GetSection("Cache"));
serviceCollection.AddMemoryCache(); // Dependency of EntityCacheMemoryService
serviceCollection.AddSingleton<IEntityCache, EntityCacheMemoryService>();
serviceCollection.AddSingleton<IEventStoreService, EventStoreDbService>();
serviceCollection.AddSingleton<IEntityService, EntityService>();
```

For this configuration, appsettings.json will look something like the following:
``` json
{
    "EventStoreDb":{
        "Url":"esdb+discover://localhost:2113?tls=false&keepAliveTimeout=10000&keepAliveInterval=10000"
    },
    "Cache": {
        "CachePeriodMinutes":"10"
    }
}
```