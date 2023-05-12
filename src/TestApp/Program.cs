using Entity_Person.PersonEvents;
using Example;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Webefinity.EventSource.Cache.Dict;
using Webefinity.EventSource.Core;
using Webefinity.EventSource.InMemoryDb;
using System.Diagnostics;

namespace ConsoleApp1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(options => {
                options.AddConsole();
            });
            serviceCollection.AddMemoryCache();
            serviceCollection.AddSingleton<IEntityCache, EntityCacheDictService>();
            serviceCollection.AddSingleton<IEventStoreService, EventStoreInMemoryService>();
            serviceCollection.AddSingleton<EntityService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var loggerService = serviceProvider.GetRequiredService<ILogger<Program>>();
            var entityService = serviceProvider.GetService<EntityService>()!;
            var entityCache = serviceProvider.GetService<IEntityCache>()!;

            var keys = Enumerable.Range(1, 50000).Select(r => Guid.NewGuid()).ToList();

            foreach (var key in keys)
            {
                var personId = key;
                {
                    var entity = await entityService.GetEntity<Person, PersonEventBase>(personId);
                    await entityService.Apply(entity, new CreatePersonEvent { Name = "Johnny", MobilePhone = "0410 003 430", EmailAddress = "johnny@cash.com" });
                    await entityService.Apply(entity, new AddPostalAddress { StreetNo = 14, StreetName = "Oak Close", City = "Nunnawadding", State = "Victoria", PostalCode = "3123" });
                    await entityService.Apply(entity, new UpdateEmailAddress { EmailAddress = "jon@cash.org" });
                }
            }

            // Force the memory cache to empty, so the item needs to be reloaded
            entityCache.EvacuateCache(true);

            {
                var start = Stopwatch.GetTimestamp();
                foreach (var key in keys)
                {
                    var personId = key;
                    await entityService.GetEntity<Person, PersonEventBase>(personId);

                }
                var time = Stopwatch.GetElapsedTime(start);
                loggerService.LogInformation($"Loading took {time.TotalMilliseconds}ms for {keys.Count} records.");
                loggerService.LogInformation($" -  or {time.TotalMicroseconds / keys.Count} microseconds per record.");

                var entity = await entityService.GetEntity<Person, PersonEventBase>(keys.Last());
                System.Diagnostics.Debug.Assert(entity.Name == "Johnny");
                System.Diagnostics.Debug.Assert(entity.PostalAddresses.Count == 1);
                System.Diagnostics.Debug.Assert(entity.PostalAddresses[0].StreetNo == 14);
            }
        }
    }
}