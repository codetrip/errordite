using Errordite.Core.Configuration;
using Errordite.Core.Domain;
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
        public string OrgId { get; set; }
        [FriendlyId]
        public string AppId { get; set; }
        [FriendlyId]
        public string IssueId { get; set; }
        public string IssueName { get; set; }

        protected string IssueUrl(ErrorditeConfiguration configuration)
        {
            return string.Format("{0}/{1}/{2}/{3}", configuration.SiteBaseUrl, IdHelper.GetFriendlyId(OrgId), IdHelper.GetFriendlyId(AppId), IdHelper.GetFriendlyId(IssueId));
        }
    }
}