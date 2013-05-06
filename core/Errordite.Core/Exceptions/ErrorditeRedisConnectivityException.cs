
using System;
using System.Runtime.Serialization;

namespace Errordite.Core.Exceptions
{
    public class ErrorditeRedisConnectivityException : ErrorditeException
    {
        public ErrorditeRedisConnectivityException()
        {}

        public ErrorditeRedisConnectivityException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public ErrorditeRedisConnectivityException(string message)
            : base (message)
        {}

        public ErrorditeRedisConnectivityException(string message, bool logged)
            : base(message, logged)
        { }

        public ErrorditeRedisConnectivityException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}