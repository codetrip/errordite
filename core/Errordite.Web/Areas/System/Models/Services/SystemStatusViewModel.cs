using System.Collections.Generic;
using System.Web.Mvc;
using Errordite.Core.Configuration;
using Errordite.Core.Domain.Central;
using Errordite.Core.Monitoring.Entities;

namespace Errordite.Web.Areas.System.Models.Services
{
    public class SystemStatusViewModel
    {
        public IList<ServiceInfoViewModel> Services { get; set; }
        public IEnumerable<SelectListItem> RavenInstances { get; set; }
        public string RavenInstanceId { get; set; }
    }

    public class ServiceInfoViewModel
    {
        public ServiceConfiguration Configuration { get; set; }
        public RavenInstance RavenInstance { get; set; }
        public ServiceStatus ServiceStatus { get; set; }
    }
}