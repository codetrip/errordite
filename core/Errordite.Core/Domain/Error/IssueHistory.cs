
using System;
using System.Collections.Generic;
using System.Web;
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
        public DateTimeOffset DateAddedUtc { get; set; }
        [ProtoMember(4)]
        public bool SystemMessage { get; set; }
        [ProtoMember(5)]
        public HistoryItemType Type { get; set; }
        [ProtoMember(6)]
        public string SpawningIssueId { get; set; }
        [ProtoMember(7)]
        public string AssignedToUserId { get; set; }
        [ProtoMember(8)]
        public IssueStatus NewStatus { get; set; }
        [ProtoMember(8)]
        public IssueStatus PreviousStatus { get; set; }
        [ProtoMember(10)]
        public IDictionary<string, int> ReprocessingResult { get; set; }
        [ProtoMember(11)]
        public string Comment { get; set; }
        [ProtoMember(12)]
        public string SpawnedIssueId { get; set; }
        [ProtoMember(13)]
        public string ExceptionType { get; set; }
        [ProtoMember(14)]
        public string ExceptionMethod { get; set; }
        [ProtoMember(15)]
        public string ExceptionMachine { get; set; }
        [ProtoMember(16)]
        public string ApplicationId { get; set; }

        public string GetMessage(IEnumerable<User> users, LocalMemoizer<string, Issue> issueMemoizer, Func<string, string> issueUrlGetter)
        {
            var user = UserId.IfPoss(id => users.FirstOrDefault(u => u.Id == id));

            switch (Type)
            {
                case HistoryItemType.CreatedByRuleAdjustment:
                    return "Issue was created by adjustment of rules of {0} by {1}.".FormatWith(issueUrlGetter(SpawningIssueId), GetUserString(user));
                case HistoryItemType.ManuallyCreated:
                    return "Issue was created manually by <span class=\"bold\">{0}</span> with status <span class=\"bold\">{1}</span>, assigned to <span class=\"bold\">{2}</span>".FormatWith(GetUserString(user), PreviousStatus, AssignedToUserId);
                case HistoryItemType.AssignedUserChanged:
                    return "Status was updated from {0} to {1} by {2}.".FormatWith(PreviousStatus, NewStatus, GetUserString(user));
                case HistoryItemType.MergedTo:
                    return "Issue was created by adjustment of rules of issue {0} by <span class=\"bold\">{1}</span>.".FormatWith(issueUrlGetter(SpawningIssueId), GetUserString(user));
                case HistoryItemType.ErrorsPurged:
                    return "All errors attached to this issue were deleted by <span class=\"bold\">{0}</span>.".FormatWith(GetUserString(user));
                case HistoryItemType.ErrorsReprocessed:
                    return "All errors associated with this issue were re-processed by <span class=\"bold\">{0}</span>.<br />{1}".FormatWith(
                        GetUserString(user),
                        new ReprocessIssueErrorsResponse { AttachedIssueIds = ReprocessingResult, Status = ReprocessIssueErrorsStatus.Ok }.GetMessage(IssueId));
                case HistoryItemType.Comment:
                    return Comment;
                case HistoryItemType.RulesAdjustedCreatedNewIssue:
                    return "Issue rules were adjusted by <span class=\"bold\">{0}</span>. Errors that no longer match this issue got attached to issue {1}.".FormatWith(GetUserString(user), issueUrlGetter(SpawnedIssueId));
                case HistoryItemType.RulesAdjustedNoNewIssue:
                    return "Issue rules were adjusted by <span class=\"bold\">{0}</span>. All errors stayed attached to this issue.".FormatWith(GetUserString(user));
                case HistoryItemType.AutoCreated:
                    return "Issue created by new error of type <span class=\"bold\">{0}</span> in method <span class=\"bold\">{1}</span> on machine <span class=\"bold\">{2}</span>".FormatWith(HttpUtility.HtmlEncode(ExceptionType), HttpUtility.HtmlEncode(ExceptionMethod), HttpUtility.HtmlEncode(ExceptionMachine));
                case HistoryItemType.StatusUpdated:
                    return "Status was updated from <span class=\"bold\">{0}</span> to <span class=\"bold\">{1}</span> by <span class=\"bold\">{2}</span>.".FormatWith(PreviousStatus, NewStatus, GetUserString(user));
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
