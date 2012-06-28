
using System.Collections.Generic;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Web.Models.Notifications
{
    public class NotificationViewModel
    {
        public string Id { get; set; }
        public NotificationType Type { get; set; }
        public string Name { get; set; }
        public IList<NotificationGroupViewModel> Groups { get; set; }
    }

    public class NotificationGroupViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }
}