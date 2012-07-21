using System;
using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using Errordite.Core.Authorisation;
using Errordite.Core.IoC;
using Errordite.Core.Matching;
using System.Linq;
using ProductionProfiler.Core.Profiling.Entities;
using ProtoBuf;
using Errordite.Core.Extensions;

namespace Errordite.Core.Domain.Error
{
    public class IssueBase
    {
        [ProtoMember(1)]
        public string Id { get; set; }
        [ProtoMember(2)]
        public IList<IMatchRule> Rules { get; set; }
        [ProtoMember(3)]
        public DateTime LastErrorUtc { get; set; }
        [ProtoMember(4)]
        public MatchPriority MatchPriority { get; set; }
        [ProtoMember(5)]
        public string ApplicationId { get; set; }

        public bool RulesEqual(IList<IMatchRule> rules)
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
    public class Issue : IssueBase, IOrganisationEntity, IWantToKnowAboutProdProf
    {
        [ProtoMember(6)]
        public ErrorLimitStatus LimitStatus { get; set; }
        [ProtoMember(7)]
        public string Name { get; set; }
        [ProtoMember(8)]
        public string UserId { get; set; }
        [ProtoMember(9)]
        public IssueStatus Status { get; set; }
        [ProtoMember(10)]
        public IList<IssueHistory> History { get; set; }
        [ProtoMember(11)]
        public int ErrorCount { get; set; }
        [ProtoMember(12)]
        public DateTime CreatedOnUtc { get; set; }
        [ProtoMember(13)]
        public DateTime LastModifiedUtc { get; set; }
        [ProtoMember(14)]
        public string OrganisationId { get; set; }
        [ProtoMember(15)]
        public IList<ProdProfRecord> ProdProfRecords { get; set; }
        [ProtoMember(16)]
        public bool TestIssue { get; set; }
        [ProtoMember(17)]
        public bool AlwaysNotify { get; set; }
        [ProtoMember(18)]
        public string Reference { get; set; }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public string FriendlyId { get { return Id == null ? string.Empty : Id.Split('/')[1]; } }

        public static string GetId(string friendlyId)
        {
            return friendlyId.Contains("/") ? friendlyId : "issues/{0}".FormatWith(friendlyId);
        }

        void IWantToKnowAboutProdProf.TellMe(ProfiledRequestData data)
        {
            ProdProfRecords.Add(new ProdProfRecord
            {
                RequestId = data.Id,
                TimestampUtc = DateTime.UtcNow,
                Url = data.Url,
            });
        }

        public Issue()
        {
            ProdProfRecords = new List<ProdProfRecord>();
        }

        public IssueBase ToIssueBase()
        {
            return new IssueBase
            {
                ApplicationId = ApplicationId,
                Id = Id,
                LastErrorUtc = LastErrorUtc,
                Rules = Rules,
                MatchPriority = MatchPriority
            };
        }
    }

    [ProtoContract]
    public class ProdProfRecord
    {
        [ProtoMember(1)]
        public string Url { get; set; }
        [ProtoMember(2)]
        public Guid RequestId { get; set; }
        [ProtoMember(3)]
        public DateTime TimestampUtc { get; set; }
    }

    public enum ErrorLimitStatus
    {
        Ok,
        Warning,
        Exceeded
    }

    public enum MatchPriority
    {
        Low = 0,
        Medium = 25,
        High = 50
    }
}