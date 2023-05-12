using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Webefinity.EventSource.Cache.Dict;
using Webefinity.EventSource.Cache.Dict.Options;
using Webefinity.EventSource.Core;
using Webefinity.EventSource.InMemoryDb;

namespace Webefinity.EventSource.Tests
{
    [TestClass]
    public class InMemory_NoCache_Tests: TestsBase
    {
        [TestInitialize]
        public void InitializeEnvironment()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(options =>
            {
                options.AddConsole();
            });

            serviceCollection.AddSingleton<IEventStoreService, EventStoreInMemoryService>();
            serviceCollection.AddSingleton<IEntityService, EntityService>();

            this.serviceProvider = serviceCollection.BuildServiceProvider();
        }

    }
}