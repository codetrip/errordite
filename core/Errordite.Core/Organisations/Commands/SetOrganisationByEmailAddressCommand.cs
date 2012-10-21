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
    public interface ISetOrganisationByEmailAddressCommand : ICommand<SetOrganisationByEmailAddressRequest, SetOrganisationByEmailAddressResponse>
    {
    }

    public class SetOrganisationByEmailAddressCommand : ComponentBase, ISetOrganisationByEmailAddressCommand
    {
        private IGetOrganisationQuery _getOrganisationQuery;
        private IAppSession _session;

        public SetOrganisationByEmailAddressCommand(IGetOrganisationQuery getOrganisationQuery, IAppSession session)
        {
            _getOrganisationQuery = getOrganisationQuery;
            _session = session;
        }

        public SetOrganisationByEmailAddressResponse Invoke(SetOrganisationByEmailAddressRequest request)
        {
            var mapping = 
            _session.CentralRaven.Query<UserOrgMapping>().FirstOrDefault(
                m => m.EmailAddress == request.EmailAddress);

            var org =
                mapping.IfPoss(m =>
                               _getOrganisationQuery.Invoke(new GetOrganisationRequest()
                                   {OrganisationId = m.OrganisationId}).Organisation);

            if (org != null)
                _session.SetOrg(org);

            return new SetOrganisationByEmailAddressResponse()
                {
                    Organisation = org,
                };
        }
    }

    public class SetOrganisationByEmailAddressRequest
    {
        public string EmailAddress { get; set; }
    }

    public class SetOrganisationByEmailAddressResponse
    {
        public Organisation Organisation { get; set; }
    }
   

}