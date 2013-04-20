using System;
using System.Text;
using System.Web.Http;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.Encryption;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations.Queries;
using Errordite.Core.Session;

namespace Errordite.Core.WebApi
{
    public abstract class ErrorditeApiController : ApiController
    {
        public IAppSession Session { protected get; set; }
        public IComponentAuditor Auditor { protected get; set; }
        public IGetOrganisationQuery GetOrganisation { protected get; set; }
        public IEncryptor Encryptor { protected get; set; }

        //TODO: do this with an action filter maybe
        protected void SetOrganisation(string orgId)
        {
            var organisation = GetOrganisation.Invoke(new GetOrganisationRequest
            {
                OrganisationId = orgId
            }).Organisation;

            Session.SetOrganisation(organisation);
        }

        protected Organisation GetOrganisationFromApiKey(string apiKey)
        {
            var token = Encryptor.Decrypt(Encoding.UTF8.GetString(Convert.FromBase64String(apiKey)));

            Auditor.Trace(GetType(), "Token decrypted to:={0}", token);

            string[] tokenParts = token.Split('|');

            if (tokenParts.Length != 2)
            {
                Auditor.Trace(GetType(), "apiKey {0} decrypts to {1} which does not have 2 separated parts.", apiKey, token);
                return null;
            }

            var organisation = GetOrganisation.Invoke(new GetOrganisationRequest
            {
                OrganisationId = Organisation.GetId(tokenParts[0])
            }).Organisation;

            //make sure we have the organisation and the salt matches
            if (organisation == null || organisation.ApiKeySalt != tokenParts[1])
                return null;
            
            Session.SetOrganisation(organisation);
            return organisation;
        }
    }
}