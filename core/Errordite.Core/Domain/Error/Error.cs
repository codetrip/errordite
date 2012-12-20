using System;
using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using ProtoBuf;
using System.Linq;

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
        public DateTime TimestampUtc { get; set; }
        [ProtoMember(4)]
        public string ApplicationId { get; set; }
        [ProtoMember(5)]
        public string OrganisationId { get; set; }
        [ProtoMember(6)]
        public string MachineName { get; set; }
        [ProtoMember(7)]
        public string Url { get; set; }
        /// <summary>
        /// Here mainly for legacy reasons - for items serialized with nested ExceptionInfos rather than flattened into a single list,
        /// we need a setter here to pull them all out on deserialisation.
        /// </summary>
        public ExceptionInfo ExceptionInfo
        {
            get { return ExceptionInfos.First(); } 
            set { ExceptionInfos = value.RecursiveGetInfos().ToArray(); }
        }
        [ProtoMember(8)]
        public string UserAgent { get; set; }
        [ProtoMember(9)]
        public bool TestError { get; set; }
        [ProtoMember(10)]
        public List<TraceMessage> Messages { get; set; }
        [ProtoMember(11)]
        public ExceptionInfo[] ExceptionInfos { get; set; }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
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
        public long Milliseconds { get; set; }
        [ProtoMember(2)]
        public string Message { get; set; }
        [ProtoMember(3)]
        public string Level { get; set; }
        [ProtoMember(4)]
        public string Logger { get; set; }
    }

    [ProtoContract]
    public class ExceptionInfo
    {
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
        /// <summary>
        /// Legacy property.  Can delete once we no longer care about errors in db with this property set.
        /// </summary>
        public ExceptionInfo InnerExceptionInfo { get; set; }

        internal IEnumerable<ExceptionInfo> RecursiveGetInfos()
        {
            IEnumerable<ExceptionInfo> ret = new[] { this };

            if (InnerExceptionInfo != null)
                ret = ret.Union(InnerExceptionInfo.RecursiveGetInfos());

            return ret;
        }
    }
}