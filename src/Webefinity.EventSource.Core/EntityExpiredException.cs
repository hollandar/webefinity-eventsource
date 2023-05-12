using System.Runtime.Serialization;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// Thrown when accessing a cached entity that has passed its expiration date.
    /// </summary>
    [Serializable]
    internal class EntityExpiredException : Exception
    {
        public EntityExpiredException()
        {
        }

        public EntityExpiredException(string? message) : base(message)
        {
        }

        public EntityExpiredException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EntityExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}