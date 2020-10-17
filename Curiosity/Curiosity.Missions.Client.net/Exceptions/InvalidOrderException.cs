using System;
using System.Runtime.Serialization;

namespace Curiosity.Missions.Client.Exceptions
{
    [Serializable]
    internal class InvalidOrderException : Exception
    {
        public InvalidOrderException()
        {
        }

        public InvalidOrderException(string message) : base(message)
        {
        }

        public InvalidOrderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidOrderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}