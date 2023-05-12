using System.Runtime.Serialization;

namespace Webefinity.EventSource.File
{
    /// <summary>
    /// Thrown when the event file format doesnt match what we expect.
    /// </summary>
    [Serializable]
    internal class EventReadException : Exception
    {
        public EventReadException()
        {
        }

        public EventReadException(string? message) : base(message)
        {
        }

        public EventReadException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EventReadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}