
using System;
using System.Runtime.Serialization;

namespace Errordite.Core.Exceptions
{
    [Serializable]
    public class ErrorditeDeleteSQSMessageException : ErrorditeException
    {
        public ErrorditeDeleteSQSMessageException()
        {}

        public ErrorditeDeleteSQSMessageException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public ErrorditeDeleteSQSMessageException(string message)
            : base (message)
        {}

        public ErrorditeDeleteSQSMessageException(string message, bool logged)
            : base(message, logged)
        { }

        public ErrorditeDeleteSQSMessageException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}