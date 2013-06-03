using System;
using System.Linq;
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