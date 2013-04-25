using System;
using System.Collections.Generic;
using Errordite.Core.Dynamic;
using Errordite.Core.Extensions;
using Errordite.Core.Authorisation;
using Errordite.Core.Matching;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;
using Errordite.Core.Extensions;

namespace Errordite.Core.Domain.Error
{
    public class IssueBase : IOrganisationEntity
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public List<IMatchRule> Rules { get; set; }
        [ProtoMember(3)]
        public DateTimeOffset? LastRuleAdjustmentUtc { get; set; }
        [ProtoMember(4)]
        public string ApplicationId { get; set; }
        [ProtoMember(5)]
        public string OrganisationId { get; set; }

        public bool RulesEqual(List<IMatchRule> rules)
        {
            if (rules.Count != Rules.Count)
                return false;

            return rules.GetHash() == Rules.GetHash();
        }

        /// <summary>
        /// MD5 hash of the rules (should be unique for any given set of rules - allows quick checking for dupes).
        /// Read-only and computed so that we don't have to remember to set it any time we change rules.
        /// </summary>
        public string RulesHash { get { return Rules.GetHash(); } }

        public bool RulesMatch(Error instance)
        {
            if (Rules.All(r => r.IsMatch(instance)))
            {
                return true;
            }

            return false;
        }
    }

    [ProtoContract]
    public class Issue : IssueBase
    {
        [ProtoMember(6)]
        public ErrorLimitStatus LimitStatus { get; set; }
        [ProtoMember(7)]
        public string Name { get; set; }
        [ProtoMember(8)]
        public string UserId { get; set; }
        [ProtoMember(9), JsonConverter(typeof(StringEnumConverter))]
        public IssueStatus Status { get; set; }
        [ProtoMember(10)]
        public int ErrorCount { get; set; }
        [ProtoMember(11)]
        public DateTimeOffset CreatedOnUtc { get; set; }
        [ProtoMember(12)]
        public DateTimeOffset LastModifiedUtc { get; set; }
        [ProtoMember(13)]
        public bool TestIssue { get; set; }
        [ProtoMember(14)]
        public bool AlwaysNotify { get; set; }
        [ProtoMember(15)]
        public string Reference { get; set; }
        [ProtoMember(16)]
        public DateTimeOffset LastErrorUtc { get; set; }
        [ProtoMember(17)]
        public DateTimeOffset? LastNotified { get; set; }

        [JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }

        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "issues/{0}".FormatWith(friendlyId);
        }

        public IssueBase ToIssueBase()
        {
            var issueBase = new IssueBase();
            PropertyMapper.Map(this, issueBase);
            return issueBase;
        }
    }

    public enum ErrorLimitStatus
    {
        Ok,
        Warning,
        Exceeded
    }
}