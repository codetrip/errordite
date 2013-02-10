using System;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using Errordite.Core.Session;
using NServiceBus.UnitOfWork;

namespace Errordite.Core.IoC
{
    public class NServiceBusHandlerUnitOfWorkManager : IManageUnitsOfWork
    {
        private readonly IWindsorContainer _windsorContainer;
        private IDisposable _containerScope;

        public NServiceBusHandlerUnitOfWorkManager(IWindsorContainer windsorContainer)
        {
            _windsorContainer = windsorContainer;
        }

        public void Begin()
        {
            _containerScope = _windsorContainer.BeginScope();
        }

        public void End(Exception ex = null)
        {
            if (ex == null)
            {
                var session = _windsorContainer.Resolve<IAppSession>();
                session.Commit();
                session.Close(); //shouldn't really have to do this - it should be disposable
            }
            _containerScope.Dispose();
        }
    }
}