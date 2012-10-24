using CodeTrip.Core;
using CodeTrip.Core.Encryption;
using CodeTrip.Core.Interfaces;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations;
using CodeTrip.Core.Extensions;

namespace Errordite.Core.Applications.Queries
{
    public class GetApplicationByTokenQuery : ComponentBase, IGetApplicationByTokenQuery
    {
        private readonly IGetApplicationQuery _getApplicationQuery;
        private readonly IEncryptor _encryptor;

        public GetApplicationByTokenQuery(IGetApplicationQuery getApplicationQuery, IEncryptor encryptor)
        {
            _getApplicationQuery = getApplicationQuery;
            _encryptor = encryptor;
        }

        public GetApplicationByTokenResponse Invoke(GetApplicationByTokenRequest request)
        {
            Trace("Starting...");

            var token = _encryptor.Decrypt(request.Token);
            string[] tokenParts = token.Split('|');

            if (!tokenParts.Length.IsIn(2, 3))
            {
                Trace("Token {0} decrypts to {1} which does not have 2 or 3 separated parts.", request.Token, token);
                //return new GetApplicationByTokenResponse();
            }

            string applicationId = Application.GetId(tokenParts[0]);
            string organisationId = tokenParts.Length == 1 ? "organisations/1" : Organisation.GetId(tokenParts[1]);

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
