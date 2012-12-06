
using Errordite.Core.Notifications.Parsing;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class IssueAssignedToUserEmailInfo : EmailInfoBase
    {
        [FriendlyId]
        public string IssueId { get; set; }
        public string IssueName { get; set; }
    }
}
