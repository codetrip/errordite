
using CodeTrip.Core.Caching.Entities;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Core.Organisations
{
    public class OrganisationRequestBase
    {
        public User CurrentUser { get; set; }
    }

    public abstract class CacheableOrganisationRequestBase<T> : CacheableRequestBase<T>
    {
        public User CurrentUser { get; set; }
    }
}
