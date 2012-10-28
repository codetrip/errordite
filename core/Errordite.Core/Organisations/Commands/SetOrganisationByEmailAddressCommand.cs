using System;
using CodeTrip.Core;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Central;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations.Queries;
using System.Linq;
using CodeTrip.Core.Extensions;
using Errordite.Core.Session;

namespace Errordite.Core.Organisations.Commands
{
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

    public class GetOrganisationByEmailAddressRequest
    {
        public string EmailAddress { get; set; }
    }

    public class GetOrganisationByEmailAddressResponse
    {
        public Organisation Organisation { get; set; }
    }
   

}