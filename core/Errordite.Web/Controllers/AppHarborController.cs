using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Master;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Dynamic;
using Errordite.Core.Identity;
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

    public class HerokuCallbackResponse
    {
        public string callback_url { get; set; }
        public Dictionary<string, string> config { get; set; }
        public string[] domains { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string owner_email { get; set; }
        public string region { get; set; }
    }

    public class AppHarborController : ErrorditeController
    {
        private const string AuthUsername = "errordite";
        private const string AuthPassword = "e882f2896c5eb768ba566f52ee2d80fa";
        private ICreateOrganisationCommand _createOrganisationCommand;
        private IDeleteOrganisationCommand _deleteOrganisationCommand;
        private IAuthenticationManager _authenticationManager;

        public AppHarborController(ICreateOrganisationCommand createOrganisationCommand, IDeleteOrganisationCommand deleteOrganisationCommand, IAuthenticationManager authenticationManager)
        {
            _createOrganisationCommand = createOrganisationCommand;
            _deleteOrganisationCommand = deleteOrganisationCommand;
            _authenticationManager = authenticationManager;
        }

        [HttpPost, ActionName("Resources")]
        public ActionResult Post(HerokuRequest request) 
        {
            if (!Authorize())
            {
                Response.StatusCode = 401;
                return Content("Unauthorized");
            }

            Trace(SummaryWriter.GetSummary(request));
            
            var org = _createOrganisationCommand.Invoke(new CreateOrganisationRequest()
                {
                    Email = request.heroku_id,
                    FirstName = "Root",
                    LastName = "AppHarbor User",
                    OrganisationName = request.heroku_id,
                    Password = Membership.GeneratePassword(10, 5),
                    SpecialUser = SpecialUser.AppHarbor,
                    CallbackUrl = request.callback_url,
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

        [HttpPut, ActionName("Resources")]
        public ActionResult Put(HerokuRequest request)
        {
            if (!Authorize())
            {
                Response.StatusCode = 401;
                return Content("Unauthorized");
            }

            //TODO: implement plan change

            return new PlainJsonNetResult(new{});
        }

        private bool Authorize()
        {
            HttpContext.Items["302to401"] = true;

            var authHeader = Request.Headers["Authorization"];

            if (authHeader == null)
                return false;

            var credentials = ParseAuthHeader(authHeader);

            return credentials[0] == AuthUsername && credentials[1] == AuthPassword;

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

            var orgId = Organisation.GetId(id);

            var mapping = Core.Session.MasterRaven.Query<UserOrganisationMapping>()
                              .FirstOrDefault(m => m.Organisations.Any(o => o == orgId) && m.SsoUser);
            
            if (mapping == null)
                throw new HttpException(403, "Organisation {0} not found".FormatWith(id));

            var org = Core.Session.MasterRaven.Load<Organisation>(orgId);

            if (org.Name == mapping.EmailAddress)
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("{0}:{1}".FormatWith(AuthUsername, AuthPassword))));

                var httpTask = client.GetAsync(org.CallbackUrl);
                httpTask.Wait();
                httpTask.Result.EnsureSuccessStatusCode();

                var contentTask = httpTask.Result.Content.ReadAsAsync<HerokuCallbackResponse>();
                contentTask.Wait();

                var callbackResponse = contentTask.Result;

                org.Name = callbackResponse.name;
            }

            var navData = Request.Form["nav-data"];
            var cookie = new HttpCookie("appharbor-nav-data", navData);

            Response.SetCookie(cookie);

            _authenticationManager.SignIn(mapping.EmailAddress);

            return Redirect("/dashboard");
        }

        private void AuthenticateToken(string id, string token, string timeStamp)
        {
            var validToken = string.Join(":", id, "3a31b4e7de2e95d545050da8b522571f", timeStamp);
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
