using Errordite.Core.Notifications.Parsing;

namespace Errordite.Core.Notifications.EmailInfo
{
    public class IssueEmailInfoBase : EmailInfoBase
    {
        public string ApplicationName { get; set; }
        public string Type { get; set; }
        public string ExceptionMessage { get; set; }
        public string Method { get; set; }
        [FriendlyId]
        public string IssueId { get; set; }
        public string IssueName { get; set; }
    }
}