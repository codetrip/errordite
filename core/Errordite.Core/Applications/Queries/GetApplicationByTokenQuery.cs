using CodeTrip.Core;
using CodeTrip.Core.Encryption;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations;
using CodeTrip.Core.Extensions;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;

namespace Errordite.Core.Applications.Queries
{
    public class GetApplicationByTokenQuery : ComponentBase, IGetApplicationByTokenQuery
    {
        private readonly IGetApplicationQuery _getApplicationQuery;
        private readonly IEncryptor _encryptor;
        private readonly IGetOrganisationQuery _getOrganisationQuery;
        private readonly IAppSession _appSession;
        public GetApplicationByTokenQuery(IGetApplicationQuery getApplicationQuery, IEncryptor encryptor, IGetOrganisationQuery getOrganisationQuery, IAppSession appSession)
        {
            _getApplicationQuery = getApplicationQuery;
            _encryptor = encryptor;
            _getOrganisationQuery = getOrganisationQuery;
            _appSession = appSession;
        }

        public GetApplicationByTokenResponse Invoke(GetApplicationByTokenRequest request)
        {
            Trace("Starting...");

            var token = _encryptor.Decrypt(request.Token);

            Trace("Token decrypted to:={0}", token);

            string[] tokenParts = token.Split('|');

            if (tokenParts.Length != 3)
            {
                Trace("Token {0} decrypts to {1} which does not have 3 separated parts.", request.Token, token);
                return new GetApplicationByTokenResponse();
            }

            string applicationId = Application.GetId(tokenParts[0]);
            string organisationId = Organisation.GetId(tokenParts[1]);

            var organisation = _getOrganisationQuery.Invoke(new GetOrganisationRequest
            {
                OrganisationId = organisationId
            }).Organisation;

            if (organisation == null)
            {
                Trace("Organisation with id {0} not found", organisationId);
            }

            _appSession.SetOrganisation(organisation);

            var application = _getApplicationQuery.Invoke(new GetApplicationRequest
            {
                Id = applicationId,
                OrganisationId = organisationId,
                CurrentUser = request.CurrentUser,
            }).Application;

            if (tokenParts.Length == 3 && application.TokenSalt != tokenParts[2])
            {
                Trace("Application {0} salt is {1} but salt in token was {2}", application.Id, application.TokenSalt, tokenParts[2]);
                return new GetApplicationByTokenResponse();
            }

            return new GetApplicationByTokenResponse
            {
                Application = application
            };
        }
    }

    public interface IGetApplicationByTokenQuery : IQuery<GetApplicationByTokenRequest, GetApplicationByTokenResponse>
    { }

    public class GetApplicationByTokenResponse
    {
        public Application Application { get; set; }
    }

    public class GetApplicationByTokenRequest : OrganisationRequestBase
    {
        public string Token { get; set; }
    }
}
