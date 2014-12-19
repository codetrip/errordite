using System;
using System.Runtime.Serialization;

namespace Errordite.Core.Exceptions
{
	[Serializable]
	public class ErrorditeCacheInvalidationException : ErrorditeException
    {
        public ErrorditeCacheInvalidationException()
        {}

        public ErrorditeCacheInvalidationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public ErrorditeCacheInvalidationException(string message)
            : base (message)
        {}

        public ErrorditeCacheInvalidationException(string message, bool logged)
            : base(message, logged)
        { }

        public ErrorditeCacheInvalidationException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}