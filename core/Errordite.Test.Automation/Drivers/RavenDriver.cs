
using System;
using System.Linq.Expressions;
using CodeTrip.Core.Session;
using Raven.Client;
using Raven.Client.Linq;

namespace Errordite.Test.Automation.Drivers
{
    public class RavenDriver
    {
        private readonly IAppSession _appSession;

        public RavenDriver(IAppSession appSession)
        {
            _appSession = appSession;
        }

        public void Delete<T>(string documentId) where T : class
        {
            var session = _appSession.Raven;
            {
                var document = session.Load<T>(documentId);
                if (document != null)
                {
                    session.Delete(document);
                }
            }
        }

        public void SaveChanges()
        {
            _appSession.Raven.SaveChanges();
        }

        public IRavenQueryable<T> QueryWithWait<T>()
        {
            return _appSession.Raven.Query<T>()
                .Customize(c => c.WaitForNonStaleResults());
        }
    }
}

