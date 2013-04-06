
using System;
using System.Runtime.Serialization;

namespace CodeTrip.Core.Exceptions
{
    [Serializable]
    public class CodeTripValidationException : CodeTripException
    {
        public CodeTripValidationException()
        {}

        public CodeTripValidationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public CodeTripValidationException(string message)
            : base (message)
        {}

        public CodeTripValidationException(string message, bool logged)
            : base(message, logged)
        { }

        public CodeTripValidationException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}