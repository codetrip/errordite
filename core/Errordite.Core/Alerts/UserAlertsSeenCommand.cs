using System.Collections.Generic;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Authorisation;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations;
using Raven.Client.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Alerts
{
    public class UserAlertsSeenCommand : SessionAccessBase, IUserAlertsSeenCommand
    {
        private readonly IAuthorisationManager _authorisationManager;

        public UserAlertsSeenCommand(IAuthorisationManager authorisationManager)
        {
            _authorisationManager = authorisationManager;
        }

        public UserAlertsSeenResponse Invoke(UserAlertsSeenRequest request)
        {
            if (request.AlertIds == null)
            {
                foreach (var alert in Session.Raven.Query<UserAlert>().Where(a => a.UserId == request.CurrentUser.Id))
                {
                    Delete(alert);
                }
            }
            else
            {
                foreach (var alertId in request.AlertIds)
                {
                    var alert = Load<UserAlert>(alertId);
                    _authorisationManager.Authorise(alert, request.CurrentUser);
                    Delete(alert);
                }
            }

            return new UserAlertsSeenResponse();
        }
    }

    public interface IUserAlertsSeenCommand : ICommand<UserAlertsSeenRequest, UserAlertsSeenResponse>
    { }

    public class UserAlertsSeenRequest : OrganisationRequestBase
    {
        public IEnumerable<string> AlertIds { get; set; }
    }

    public class UserAlertsSeenResponse
    {}
}