using System.Collections.Generic;
using Castle.Core;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interceptors;
using Errordite.Core.Domain.Master;
using Errordite.Core.Interfaces;
using Errordite.Core.Caching;
using Errordite.Core.Domain.Organisation;
using System.Linq;
using Errordite.Core.Session;
using ProtoBuf;

namespace Errordite.Core.Organisations.Queries
{
    [Interceptor(CacheInterceptor.IoCName)]
    public class GetOrganisationsByEmailAddressCommand : ComponentBase, IGetOrganisationsByEmailAddressCommand
    {
        private readonly IGetOrganisationQuery _getOrganisationQuery;
        private readonly IAppSession _session;

        public GetOrganisationsByEmailAddressCommand(IGetOrganisationQuery getOrganisationQuery, IAppSession session)
        {
            _getOrganisationQuery = getOrganisationQuery;
            _session = session;
        }

        public GetOrganisationsByEmailAddressResponse Invoke(GetOrganisationsByEmailAddressRequest request)
        {
            var mapping = _session.MasterRaven.Query<UserOrganisationMapping>().FirstOrDefault(m => m.EmailAddress == request.EmailAddress);

			if (mapping == null)
			{
				return new GetOrganisationsByEmailAddressResponse
				{
					Organisations = new List<Organisation>()
				};
			}

	        var organisations = mapping.Organisations.Select(id => _getOrganisationQuery.Invoke(new GetOrganisationRequest
		    {
			    OrganisationId = id
		    }).Organisation);

            return new GetOrganisationsByEmailAddressResponse
            {
				Organisations = organisations,
				UserMapping = mapping
            };
        }
    }

    public interface IGetOrganisationsByEmailAddressCommand : ICommand<GetOrganisationsByEmailAddressRequest, GetOrganisationsByEmailAddressResponse>
    { }

    public class GetOrganisationsByEmailAddressRequest : CacheableRequestBase<GetOrganisationsByEmailAddressResponse>
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
    public class GetOrganisationsByEmailAddressResponse
    {
        [ProtoMember(1)]
        public IEnumerable<Organisation> Organisations { get; set; }
		[ProtoMember(2)]
		public UserOrganisationMapping UserMapping { get; set; }
    }
}