using System;
using System.Runtime.Serialization;

namespace CodeTrip.Core.Exceptions
{
	[Serializable]
	public class CodeTripCacheException : CodeTripException
    {
        public CodeTripCacheException()
        {}

        public CodeTripCacheException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public CodeTripCacheException(string message)
            : base (message)
        {}

        public CodeTripCacheException(string message, bool logged)
            : base(message, logged)
        { }

        public CodeTripCacheException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}