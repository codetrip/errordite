using System;
using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using Newtonsoft.Json;
using ProtoBuf;
using System.Linq;

namespace Errordite.Core.Domain.Error
{
    [ProtoContract]
    public abstract class ErrorBase
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public string IssueId { get; set; }
        [ProtoMember(3)]
        public DateTime TimestampUtc { get; set; }
        [ProtoMember(4)]
        public string ApplicationId { get; set; }
    }

    [ProtoContract]
    public class UnloggedError : ErrorBase
    {
        public UnloggedError()
        {}

        public UnloggedError(Error error)
        {
            IssueId = error.IssueId;
            TimestampUtc = error.TimestampUtc;
        }
    }

    [ProtoContract]
    public class Error : ErrorBase
    {
        [ProtoMember(5)]
        public string OrganisationId { get; set; }
        [ProtoMember(6)]
        public string MachineName { get; set; }
        [ProtoMember(7)]
        public bool Classified { get; set; }
        [ProtoMember(8)]
        public string Url { get; set; }
        [ProtoMember(9)]
        public ExceptionInfo ExceptionInfo { get; set; }
        [ProtoMember(10)]
        public string UserAgent { get; set; }
        [ProtoMember(11)]
        public bool TestError { get; set; }
        [ProtoMember(12)]
        public List<TraceMessage> Messages { get; set; }

        /// <summary>
        /// The inner exception infos flattened into an IEnumerable, used for querying in the Errors_Search index.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<ExceptionInfo> ExceptionInfos
        {
            get
            {
                return ExceptionInfo == null ? new ExceptionInfo[0] : ExceptionInfo.RecursiveGetInfos();
            }
        }

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
        [ProtoMember(7)]
        public ExceptionInfo InnerExceptionInfo { get; set; }

        internal IEnumerable<ExceptionInfo> RecursiveGetInfos()
        {
            IEnumerable<ExceptionInfo> ret = new[] {this};

            if (InnerExceptionInfo != null)
                ret = ret.Union(InnerExceptionInfo.RecursiveGetInfos());

            return ret;
        }
    }

    public class TestError : Error
    { }
}