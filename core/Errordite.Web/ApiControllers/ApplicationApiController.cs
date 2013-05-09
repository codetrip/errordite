using System.Net;
using System.Net.Http;
using System.Web.Http;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Core.Web;

namespace Errordite.Web.ApiControllers
{
    public class ApplicationApiController : ErrorditeApiController
    {
        public Application GetApplication(string id, [FromUri]string apiKey)
        {
            var organisation = GetOrganisationFromApiKey(apiKey);

            if (organisation == null)
            {
                Auditor.Trace(GetType(), "...Failed to authenticate with apiKey:={0}", apiKey);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }

            var application = Session.Raven.Load<Application>(Application.GetId(id));

            //make sure we dont allow someone else's issue  to be accessed
            if (application == null || application.OrganisationId != organisation.Id)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            return new Application
            {
                Id = application.Id,
                Name = application.Name,
                Version = application.Version,
                IsActive = application.IsActive,
                OrganisationId = application.OrganisationId,
                Token = application.Token,
				HipChatRoomId = application.HipChatRoomId,
				CampfireRoomId = application.CampfireRoomId
            };
        }

        public HttpResponseMessage PutApplication(string id, [FromUri]string apiKey, [FromBody]Application application)
        {
            Auditor.Trace(GetType(), "Started...");
            Auditor.Trace(GetType(), "...Attempting to load organisation from apiKey key:={0}", apiKey);

            var organisation = GetOrganisationFromApiKey(apiKey);

            if (organisation == null)
            {
                Auditor.Trace(GetType(), "...Failed to authenticate with apiKey:={0}", apiKey);
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            Auditor.Trace(GetType(), "...Successfully loaded organisation Name:={0}, Id:={1}", organisation.Name, organisation.Id);

            var storedApplication = Session.Raven.Load<Application>(Application.GetId(id));

            if (storedApplication == null || storedApplication.OrganisationId != organisation.Id)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            if (application.Version.IsNotNullOrEmpty())
                storedApplication.Version = application.Version;

            storedApplication.IsActive = application.IsActive;
            storedApplication.Name = application.Name;

            return Request.CreateResponse(HttpStatusCode.OK, new Application
            {
                Id = storedApplication.Id,
                Name = storedApplication.Name,
                Version = storedApplication.Version,
                IsActive = storedApplication.IsActive,
				HipChatRoomId = storedApplication.HipChatRoomId,
                OrganisationId = storedApplication.OrganisationId,
                Token = storedApplication.Token,
                CampfireRoomId = storedApplication.CampfireRoomId
            });
        }
    }
}
