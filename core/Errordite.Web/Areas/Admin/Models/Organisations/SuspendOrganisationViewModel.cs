
using Errordite.Core.Domain.Organisation;

namespace Errordite.Web.Areas.Admin.Models.Organisations
{
    public class SuspendOrganisationViewModel
    {
        public SuspendedReason Reason { get; set; }
        public string Message { get; set; }
        public string OrganisationId { get; set; }
    }
}