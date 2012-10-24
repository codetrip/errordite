
using System;
using System.Runtime.Serialization;

namespace CodeTrip.Core.Exceptions
{
	[Serializable]
	public class CodeTripConfigurationException : CodeTripException
    {
        public CodeTripConfigurationException()
        {}

        public CodeTripConfigurationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public CodeTripConfigurationException(string message)
            : base (message)
        {}

        public CodeTripConfigurationException(string message, bool logged)
            : base(message, logged)
        { }

        public CodeTripConfigurationException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}