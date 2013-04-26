
using System;
using System.Runtime.Serialization;

namespace Errordite.Core.Exceptions
{
    [Serializable]
    public class ErrorditeValidationException : ErrorditeException
    {
        public ErrorditeValidationException()
        {}

        public ErrorditeValidationException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {}

        public ErrorditeValidationException(string message)
            : base (message)
        {}

        public ErrorditeValidationException(string message, bool logged)
            : base(message, logged)
        { }

        public ErrorditeValidationException(string message, bool logged, Exception innerException)
            : base(message, logged, innerException)
        {}
    }
}