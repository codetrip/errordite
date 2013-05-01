using System;

namespace Errordite.Core.Auditing.Entities
{
    public class NoException : Exception
    {
        public override string Message
        {
            get { return "No exception for this error"; }
        }
    }
}