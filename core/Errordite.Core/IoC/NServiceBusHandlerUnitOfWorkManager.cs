using System;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using CodeTrip.Core.Auditing.Entities;
using Errordite.Client;
using Errordite.Core.Session;
using NServiceBus.UnitOfWork;

namespace Errordite.Core.IoC
{
    public class NServiceBusHandlerUnitOfWorkManager : IManageUnitsOfWork
    {
        private readonly IWindsorContainer _windsorContainer;

        public NServiceBusHandlerUnitOfWorkManager(IWindsorContainer windsorContainer)
        {
            _windsorContainer = windsorContainer;
        }

        public void Begin()
        {
            
        }

        public void End(Exception ex = null)
        {
            var a = _windsorContainer.Resolve<IComponentAuditor>();
            try
            {
                //TODO - not sure why this isn't injected
                var session = _windsorContainer.Resolve<IAppSession>();
                if (ex == null)
                {
                    session.Commit();
                }
                else
                {
                    a.Trace(GetType(), "Exception passed to End()");
                    a.Error(GetType(), ex);
                }
                session.Close();
            }
            catch (Exception ex1)
            {
                ErrorditeClient.ReportException(ex1);
                a.Trace(GetType(), "Exception in UoW");
                a.Error(GetType(), ex1);
                throw;
            }
        }
    }
}