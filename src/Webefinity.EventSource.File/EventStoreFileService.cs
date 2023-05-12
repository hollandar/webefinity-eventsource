using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Webefinity.EventSource.Core;
using Webefinity.EventSource.File.Options;
using System.Text;

namespace Webefinity.EventSource.File
{
    /// <summary>
    /// An event store which is able to store events on disk in binary format.
    /// </summary>
    public class EventStoreFileService : IEventStoreService
    {
        private IOptions<EventStoreFileOptions> options;
        private readonly ILogger<EventStoreFileService>? logger;
        private ReaderWriterLockSlim readWriteLock = new();

        /// <summary>
        /// Construct a new file store.
        /// </summary>
        /// <param name="options">Options for file storage.</param>
        /// <param name="logger">The logger.</param>
        public EventStoreFileService(IOptions<EventStoreFileOptions> options, ILogger<EventStoreFileService>? logger)
        {
            this.options = options;
            this.logger = logger;

            logger.FastLog(LogLevel.Information, "Storing events using files at {0}".LogFormat(options.Value.Folder));

        }

        /// <summary>
        /// Given a stream, get the filename for the stream.
        /// </summary>
        /// <param name="stream">The stream name</param>
        /// <returns></returns>
        protected string GetFilename(string stream)
        {
            return Path.Combine(this.options.Value.Folder, $"{stream}.bin");
        }

        /// <inheritdoc/>
        public Task<long> AppendToStreamAsync(string stream, string eventType, long version, string json)
        {
            try
            {
                readWriteLock.EnterWriteLock();
                using var file = new FileStream(GetFilename(stream), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var content = Encoding.UTF8.GetBytes(json);

                using var memoryStream = new MemoryStream();
                using var memoryWriter = new BinaryWriter(memoryStream);

                memoryWriter.Write(eventType);
                memoryWriter.Write(version);
                memoryWriter.Write(content.Length);
                memoryWriter.Write(content);
                memoryWriter.Write(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                file.Seek(0, SeekOrigin.End);

                using var fileWriter = new BinaryWriter(file);

                fileWriter.Write(new byte[] { 0x0a, 0xa0, 0xaa, 0xaa });
                fileWriter.Write((int)memoryStream.Length);
                fileWriter.Write(memoryStream.ToArray());
                fileWriter.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 });

                return Task.FromResult(version);
            }
            finally
            {
                readWriteLock.ExitWriteLock();
            }
        }

#pragma warning disable CS1998

        /// <inheritdoc/>
        public async IAsyncEnumerable<EventContainer> ReadStreamAsync(string stream)
        {
            try
            {
                readWriteLock.EnterReadLock();
                var fileName = GetFilename(stream);
                if (System.IO.File.Exists(fileName))
                {
                    using var file = new FileStream(GetFilename(stream), FileMode.Open, FileAccess.Read);
                    using var fileReader = new BinaryReader(file);

                    while (true)
                    {
                        var header = fileReader.ReadBytes(4);
                        if (header.Length < 4) break;
                        if (!Enumerable.SequenceEqual(header, new byte[] { 0x0a, 0xa0, 0xaa, 0xaa }))
                        {
                            throw new EventReadException("Header not correct.");
                        }

                        var length = fileReader.ReadInt32();
                        var content = fileReader.ReadBytes(length);
                        var footer = fileReader.ReadBytes(4);
                        if (!Enumerable.SequenceEqual(footer, new byte[] { 0x00, 0x00, 0x00, 0x00 }))
                        {
                            throw new EventReadException("Footer not correct.");
                        }

                        using var memoryStream = new MemoryStream(content);
                        using var memoryReader = new BinaryReader(memoryStream);

                        var eventType = memoryReader.ReadString();
                        var version = memoryReader.ReadInt64();
                        var contentLength = memoryReader.ReadInt32();
                        var jsonBytes = memoryReader.ReadBytes(contentLength);
                        var json = Encoding.UTF8.GetString(jsonBytes);
                        var timestamp = memoryReader.ReadInt64();

                        var eventContainer = new EventContainer(
                            eventType, 
                            version, 
                            json, 
                            DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
                        );

                        yield return eventContainer;
                    }
                }
            }
            finally
            {
                readWriteLock.ExitReadLock();
            }
        }

#pragma warning restore CS1998

    }
}
