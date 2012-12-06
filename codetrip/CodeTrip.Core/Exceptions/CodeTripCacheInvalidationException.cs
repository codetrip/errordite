using System;
using System.Runtime.Serialization;

namespace CodeTrip.Core.Exceptions
{
	[Serializable]
	public class CodeTripCacheInvalidationException : CodeTripException
    {
        public CodeTripCacheInvalidationException()
        {}

        public CodeTripCacheInvalidationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public CodeTripCacheInvalidationException(string message)
            : base (message)
        {}

        public CodeTripCacheInvalidationException(string message, bool logged)
            : base(message, logged)
        { }

        public CodeTripCacheInvalidationException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}