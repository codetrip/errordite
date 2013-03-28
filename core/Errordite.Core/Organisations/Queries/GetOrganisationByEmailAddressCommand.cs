using Castle.Core;
using CodeTrip.Core;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interceptors;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Central;
using Errordite.Core.Domain.Organisation;
using System.Linq;
using CodeTrip.Core.Extensions;
using Errordite.Core.Session;
using ProtoBuf;

namespace Errordite.Core.Organisations.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetOrganisationByEmailAddressCommand : ComponentBase, IGetOrganisationByEmailAddressCommand
    {
        private readonly IGetOrganisationQuery _getOrganisationQuery;
        private readonly IAppSession _session;

        public GetOrganisationByEmailAddressCommand(IGetOrganisationQuery getOrganisationQuery, IAppSession session)
        {
            _getOrganisationQuery = getOrganisationQuery;
            _session = session;
        }

        public GetOrganisationByEmailAddressResponse Invoke(GetOrganisationByEmailAddressRequest request)
        {
            var mapping = _session.MasterRaven.Query<UserOrganisationMapping>().FirstOrDefault(m => m.EmailAddress == request.EmailAddress);

            var org = mapping.IfPoss(m => _getOrganisationQuery.Invoke(new GetOrganisationRequest
            {
                OrganisationId = m.OrganisationId
            }).Organisation);

            return new GetOrganisationByEmailAddressResponse
            {
                Organisation = org,
            };
        }
    }

    public interface IGetOrganisationByEmailAddressCommand : ICommand<GetOrganisationByEmailAddressRequest, GetOrganisationByEmailAddressResponse>
    { }

    public class GetOrganisationByEmailAddressRequest : CacheableRequestBase<GetOrganisationByEmailAddressResponse>
    {
        public string EmailAddress { get; set; }

        protected override string GetCacheKey()
        {
            return CacheKeys.Organisations.Email(EmailAddress);
        }

        protected override CacheProfiles GetCacheProfile()
        {
            return CacheProfiles.Organisations;
        }
    }

    [ProtoContract]
    public class GetOrganisationByEmailAddressResponse
    {
        [ProtoMember(1)]
        public Organisation Organisation { get; set; }
    }
}