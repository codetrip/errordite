using System;
using System.Linq;
using Errordite.Client;
using Errordite.Client.Configuration;
using Errordite.Client.Web;
using Errordite.Core.Domain.Error;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Session;
using Errordite.Test.Automation.Tests;

namespace Errordite.Test.Automation.Drivers
{
    public class ErrorditeClientDriver
    {
        private readonly IAppSession _appSession;

        public ErrorditeClientDriver(IAppSession appSession)
        {
            _appSession = appSession;
        }

        public Error SendException(Application app, Exception exception)
        {
            var now = DateTime.UtcNow;

            Console.WriteLine(now);

            try
            {
                throw exception;
            }
            catch (Exception ex)
            {
                ErrorditeWebRequest.To("http://dev-reception.errordite.com/receiveerror")
                .WithError(ErrorditeClient.GetClientError(ex, new ErrorditeConfiguration()
                {
                    Enabled = true,
                    Token = app.Token,
                }))
                .Send(true);    
            }
            

            Error error = null;
            Wait.ThenAssert(
                () =>
                (error = _appSession.Raven.Query<Error>()
                .Customize(c => c.WaitForNonStaleResultsAsOfNow())
                .FirstOrDefault(e => e.ApplicationId == app.Id && e.TimestampUtc > now)) != null,
                10, "Error not logged");

            return error;
        }     
    }
}