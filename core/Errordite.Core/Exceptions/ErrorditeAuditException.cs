
using System;
using System.Runtime.Serialization;

namespace Errordite.Core.Exceptions
{
	[Serializable]
    public class ErrorditeAuditException : ErrorditeException
    {
        public ErrorditeAuditException()
        {}

        public ErrorditeAuditException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public ErrorditeAuditException(string message)
            : base (message)
        {}

        public ErrorditeAuditException(string message, bool logged)
            : base(message, logged)
        { }

        public ErrorditeAuditException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}