using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Webefinity.EventSource.Cache.Dict;
using Webefinity.EventSource.Cache.Dict.Options;
using Webefinity.EventSource.Core;
using Webefinity.EventSource.File;
using Webefinity.EventSource.File.Options;
using Webefinity.EventSource.InMemoryDb;
using System.Runtime.CompilerServices;

namespace Webefinity.EventSource.Tests
{
    [TestClass]
    public class File_NoCache_Tests: TestsBase
    {
        DirectoryInfo? folder = null;

        [TestInitialize]
        public void InitializeEnvironment()
        {
            folder = Directory.CreateTempSubdirectory();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(options =>
            {
                options.AddConsole();
            });
            serviceCollection.Configure<EventStoreFileOptions>(configuration => configuration.Folder = folder.FullName);
            serviceCollection.AddSingleton<IEventStoreService, EventStoreFileService>();
            serviceCollection.AddSingleton<IEventStoreService, EventStoreInMemoryService>();
            serviceCollection.AddSingleton<IEntityService, EntityService>();

            this.serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (folder != null)
            {
                Directory.Delete(folder.FullName, true);
            }
        }

    }
}