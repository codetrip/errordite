
using System;
using System.Collections.Generic;
using CodeTrip.Core;
using CodeTrip.Core.Extensions;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Issues.Commands;
using ProtoBuf;
using System.Linq;

namespace Errordite.Core.Domain.Error
{
    [ProtoContract]
    public class IssueHistory
    {
        [ProtoMember(1)]
        public string IssueId { get; set; }
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
        public string ExceptionMachine { get; set; }

        public string GetMessage(IEnumerable<User> users, LocalMemoizer<string, Issue> issueMemoizer, Func<string, string> issueUrlGetter)
        {
            var user = UserId.IfPoss(id => users.FirstOrDefault(u => u.Id == id));

            switch (Type)
            {
                case HistoryItemType.CreatedByRuleAdjustment:
                    return "Issue was created by adjustment of rules of {0} by {1}.".FormatWith(issueUrlGetter(SpawningIssueId), GetUserString(user));
                case HistoryItemType.ManuallyCreated:
                    return "Issue was created manually by {0} with status {1}, assigned to {2}".FormatWith(GetUserString(user), PreviousStatus, AssignedToUserId);
                case HistoryItemType.AssignedUserChanged:
                    return "Status was updated from {0} to {1} by {2}.".FormatWith(PreviousStatus, NewStatus, GetUserString(user));
                case HistoryItemType.MergedTo:
                    return "Issue was created by adjustment of rules of {0} by {1}.".FormatWith(issueUrlGetter(SpawnedIssueId), issueMemoizer.Get(SpawningIssueId).IfPoss(i => i.Name, "DELETED"));
                case HistoryItemType.ErrorsPurged:
                    return "All errors attached to this issue were deleted by {0}.".FormatWith(GetUserString(user));
                case HistoryItemType.ErrorsReprocessed:
                    return "All errors associated with this issue were re-processed by {0}.<br />{1}".FormatWith(
                        GetUserString(user),
                        new ReprocessIssueErrorsResponse { AttachedIssueIds = ReprocessingResult, Status = ReprocessIssueErrorsStatus.Ok }.GetMessage(IssueId));
                case HistoryItemType.Comment:
                    return Comment;
                case HistoryItemType.RulesAdjustedCreatedNewIssue:
                    return "Issue rules were adjusted by {0}. Errors that no longer match this issue got attached to issue {1}.".FormatWith(GetUserString(user), issueUrlGetter(SpawnedIssueId));
                case HistoryItemType.RulesAdjustedNoNewIssue:
                    return "Issue rules were adjusted by {0}. All errors stayed attached to this issue.".FormatWith(GetUserString(user));
                case HistoryItemType.AutoCreated:
                    return "Issue created by new error of type <strong>{0}</strong> in method <strong>{1}</strong> on machine <strong>{2}</strong>".FormatWith(ExceptionType, ExceptionMethod, ExceptionMachine);
                case HistoryItemType.StatusUpdated:
                    return "Status was updated from {0} to {1} by {2}.".FormatWith(PreviousStatus, NewStatus, GetUserString(user));
                default:
                    return "No message";
            }
        }

        private string GetUserString(User user)
        {
            if (user == null)
                return "DELETED USER";

            return "{0} ({1})".FormatWith(user.FullName, user.Email);
        }
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
        [Obsolete("Replaced by AssignedUserChanged and StatusUpdated")]
        BatchStatusUpdate,
        Comment,
        [Obsolete("Replaced by AssignedUserChanged and StatusUpdated")]
		DetailsUpdated,
        RulesAdjustedNoNewIssue,
        AssignedUserChanged,
        StatusUpdated
    }
}
