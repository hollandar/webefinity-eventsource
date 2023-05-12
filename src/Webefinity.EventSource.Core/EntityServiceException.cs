using System.Runtime.Serialization;

namespace Webefinity.EventSource.Core
{
    /// <summary>
    /// An exception thrown if the entity layer is misconfigured.
    /// </summary>
    [Serializable]
    internal class EntityServiceException : Exception
    {
        public EntityServiceException()
        {
        }

        public EntityServiceException(string? message) : base(message)
        {
        }

        public EntityServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EntityServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}