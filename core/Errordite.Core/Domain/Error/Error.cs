using System;
using System.Collections.Generic;
using Errordite.Core.Extensions;
using ProtoBuf;
using Raven.Imports.Newtonsoft.Json;

namespace Errordite.Core.Domain.Error
{
	[ProtoContract]
    public class Error
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public string IssueId { get; set; }
        [ProtoMember(3)]
        public DateTimeOffset TimestampUtc { get; set; }
        [ProtoMember(4)]
        public string ApplicationId { get; set; }
        [ProtoMember(5)]
        public string OrganisationId { get; set; }
        [ProtoMember(6)]
        public string MachineName { get; set; }
        [ProtoMember(7)]
        public string Url { get; set; }
        [ProtoMember(8)]
        public string UserAgent { get; set; }
        [ProtoMember(9)]
        public bool TestError { get; set; }
        [ProtoMember(10)]
        public List<TraceMessage> Messages { get; set; }
        [ProtoMember(11)]
        public ExceptionInfo[] ExceptionInfos { get; set; }
        [ProtoMember(12)]
        public string Version { get; set; }
        [ProtoMember(12)]
		public Dictionary<string, string> ContextData { get; set; }

        [JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }

        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "errors/{0}".FormatWith(friendlyId);
        }
    }

    [ProtoContract]
    public class TraceMessage
    {
        [ProtoMember(1)]
        public DateTimeOffset Timestamp { get; set; }
        [ProtoMember(2)]
        public string Message { get; set; }
    }

    [ProtoContract]
    public class ExceptionInfo
    {
        public ExceptionInfo()
        {
            ExtraData = new Dictionary<string, string>();
        }

        [ProtoMember(1)]
        public string Type { get; set; }
        [ProtoMember(2)]
        public string Message { get; set; }
        [ProtoMember(3)]
        public string StackTrace { get; set; }
        [ProtoMember(4)]
        public Dictionary<string, string> ExtraData { get; set; }
        [ProtoMember(5)]
        public string MethodName { get; set; }
        [ProtoMember(6)]
        public string Module { get; set; }
    }
}