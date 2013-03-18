
using System;
using System.Runtime.Serialization;

namespace CodeTrip.Core.Exceptions
{
	[Serializable]
    public class CodeTripAuditException : CodeTripException
    {
        public CodeTripAuditException()
        {}

        public CodeTripAuditException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public CodeTripAuditException(string message)
            : base (message)
        {}

        public CodeTripAuditException(string message, bool logged)
            : base(message, logged)
        { }

        public CodeTripAuditException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}