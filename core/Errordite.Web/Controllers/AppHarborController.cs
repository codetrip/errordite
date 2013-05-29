using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Organisations.Commands;
using Errordite.Web.ActionResults;
using Errordite.Core.Extensions;
using Raven.Abstractions.Extensions;

namespace Errordite.Web.Controllers
{
    public class HerokuRequest
    {
        public string heroku_id { get; set; }
        public string plan { get; set; }
        public string callback_url { get; set; }
        public string logplex_token { get; set; }
        public dynamic options { get; set; }
    }

    public class AppHarborController : ErrorditeController
    {
        private ICreateOrganisationCommand _createOrganisationCommand;
        private IDeleteOrganisationCommand _deleteOrganisationCommand;

        public AppHarborController(ICreateOrganisationCommand createOrganisationCommand, IDeleteOrganisationCommand deleteOrganisationCommand)
        {
            _createOrganisationCommand = createOrganisationCommand;
            _deleteOrganisationCommand = deleteOrganisationCommand;
        }

        [HttpPost, ActionName("Resources")]
        public ActionResult Post(HerokuRequest request) 
        {
            if (!Authorize())
            {
                Response.StatusCode = 401;
                return Content("Unauthorized");
            }

            var org = _createOrganisationCommand.Invoke(new CreateOrganisationRequest()
                {
                    Email = request.heroku_id,
                    FirstName = "Root",
                    LastName = "AppHarbor User",
                    OrganisationName = request.heroku_id,
                    Password = Membership.GeneratePassword(10, 5),
                });

            if (org.Status != CreateOrganisationStatus.Ok)
            {
                Trace(org.Status.ToString());
                Response.StatusCode = 400;
                return Content(org.Status.ToString());
            }

            //create organisation & application with AppHarbor user
            var application = Core.Session.Raven.Load<Application>(org.ApplicationId);
            return new PlainJsonNetResult(new{id = IdHelper.GetFriendlyId(org.OrganisationId), config = new {ERRORDITE_TOKEN = application.Token, ERRORDITE_URL = "https://www.errordite.com/receiveerror"}});
        }

        [HttpDelete, ActionName("Resources")]
        public ActionResult Delete(string id)
        {
            if (!Authorize())
            {
                Response.StatusCode = 401;
                return Content("Unauthorized");
            }

            //this does not work currently - no session set
            //TODO: consider more checks that this is an AH org
            _deleteOrganisationCommand.Invoke(new DeleteOrganisationRequest() {OrganisationId = Organisation.GetId(id)});

            return new PlainJsonNetResult(new {});
        }

        private bool Authorize()
        {
            var authHeader = Request.Headers["Authorization"];

            if (authHeader == null)
                return false;

            var credentials = ParseAuthHeader(authHeader);

            return credentials[0] == "errordite" && credentials[1] == "e882f2896c5eb768ba566f52ee2d80fa";

        }

        private string[] ParseAuthHeader(string authHeader)
        {
            // Check this is a Basic Auth header
            if (authHeader == null || authHeader.Length == 0 || !authHeader.StartsWith("Basic")) return null;

            // Pull out the Credentials with are seperated by ':' and Base64 encoded
            string base64Credentials = authHeader.Substring(6);
            string[] credentials = Encoding.ASCII.GetString(Convert.FromBase64String(base64Credentials)).Split(new char[] { ':' });

            if (credentials.Length != 2 || string.IsNullOrEmpty(credentials[0]) || string.IsNullOrEmpty(credentials[0])) return null;

            // Okay this is the credentials
            return credentials;
        }

        public ActionResult Sso(string id, string timestamp, string token)
        {
            AuthenticateToken(id, token, timestamp);

            var navData = Request.QueryString["nav-data"];
            var cookie = new HttpCookie("appharbor-nav-data", navData);

            Response.SetCookie(cookie);

            return Content("id");
        }

        private void AuthenticateToken(string id, string token, string timeStamp)
        {
            var validToken = string.Join(":", id.ToString(), "3a31b4e7de2e95d545050da8b522571f", timeStamp);
            var hash = ToHash<SHA1Managed>(validToken);
            if (token != hash)
            {
                throw new HttpException(403, "Authentication failed");
            }

            var validTime = (DateTime.UtcNow.AddMinutes(-5) - new DateTime(1970, 1, 1)).TotalSeconds;
            if (Convert.ToInt32(timeStamp) < validTime)
            {
                throw new HttpException(403, "Timestamp too old");
            }
        }

        private static string ToHash<T>(string value) where T : HashAlgorithm, new()
        {
            using (HashAlgorithm hashAlgorithm = new T())
            {
                var valueBytes = Encoding.UTF8.GetBytes(value);

                var hashBytes = hashAlgorithm.ComputeHash(valueBytes);

                return BitConverter
                    .ToString(hashBytes)
                    .ToLower()
                    .Replace("-", "");
            }
        }
    }
}
