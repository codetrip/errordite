
using System;
using System.Runtime.Serialization;

namespace CodeTrip.Core.Exceptions
{
    public class CodeTripRedisConnectivityException : CodeTripException
    {
        public CodeTripRedisConnectivityException()
        {}

        public CodeTripRedisConnectivityException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public CodeTripRedisConnectivityException(string message)
            : base (message)
        {}

        public CodeTripRedisConnectivityException(string message, bool logged)
            : base(message, logged)
        { }

        public CodeTripRedisConnectivityException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}