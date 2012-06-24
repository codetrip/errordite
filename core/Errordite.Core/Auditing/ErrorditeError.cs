using System;
using System.Collections.Generic;

namespace Errordite.Core.Auditing
{
    public class ErrorditeError
    {
        public string Id { get; set; }
        public string Application { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string Message { get; set; }
        public string Module { get; set; }
        public string Method { get; set; }
        public string Machine { get; set; }
        public string User { get; set; }
        public Guid? MessageId { get; set; }
        public DateTime TimestampUtc { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }
}