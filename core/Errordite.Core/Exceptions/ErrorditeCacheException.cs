using System;
using System.Runtime.Serialization;

namespace Errordite.Core.Exceptions
{
	[Serializable]
	public class ErrorditeCacheException : ErrorditeException
    {
        public ErrorditeCacheException()
        {}

        public ErrorditeCacheException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public ErrorditeCacheException(string message)
            : base (message)
        {}

        public ErrorditeCacheException(string message, bool logged)
            : base(message, logged)
        { }

        public ErrorditeCacheException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}