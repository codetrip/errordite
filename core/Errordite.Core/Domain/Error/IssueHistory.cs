
using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Errordite.Core.Domain.Error
{
    [ProtoContract]
    public class IssueHistory
    {
        [ProtoMember(1)]
        [Obsolete("Only kept for existing issues where this is set but not the other properties.  Now we just store ids and build up the message at view-time")]
        public string Message { get; set; }
        [ProtoMember(2)]
        public string UserId { get; set; }
        [ProtoMember(3)]
        public DateTime DateAddedUtc { get; set; }
        [ProtoMember(4)]
        public bool SystemMessage { get; set; }
        [ProtoMember(5)]
        public string Reference { get; set; }
        [ProtoMember(6)]
        public HistoryItemType Type { get; set; }
        [ProtoMember(7)]
        public string SpawningIssueId { get; set; }
        [ProtoMember(8)]
        public string AssignedToUserId { get; set; }
        [ProtoMember(9)]
        public IssueStatus NewStatus { get; set; }
        [ProtoMember(10)]
        public IssueStatus PreviousStatus { get; set; }
        [ProtoMember(11)]
        public IDictionary<string, int> ReprocessingResult { get; set; }
        [ProtoMember(12)]
        public string Comment { get; set; }
        [ProtoMember(13)]
        public string SpawnedIssueId { get; set; }
        [ProtoMember(14)]
        public string ExceptionType { get; set; }
        [ProtoMember(15)]
        public string ExceptionMethod { get; set; }
        [ProtoMember(16)]
        public string ExceptionModule { get; set; }
        [ProtoMember(17)]
        public string ExceptionMachine { get; set; }
    }

    public enum HistoryItemType
    {
        CreatedByRuleAdjustment,    
        ManuallyCreated,
        MergedTo,
        ErrorsPurged,
        ErrorsReprocessed,
        RulesAdjustedCreatedNewIssue,
        AutoCreated,
        BatchStatusUpdate,
        Comment,
		DetailsUpdated
    }
}
