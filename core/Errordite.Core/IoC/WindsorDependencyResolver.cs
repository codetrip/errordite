using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Castle.Windsor;
using System.Linq;

namespace Errordite.Core.IoC
{
    public class WindsorDependencyResolver : IDependencyResolver
    {
        public WindsorDependencyResolver(IWindsorContainer windsorContainer)
        {
            WindsorContainer = windsorContainer;
        }

        public IWindsorContainer WindsorContainer { get; private set; }

        public void Dispose()
        {
            WindsorContainer.Dispose();
        }

        public object GetService(Type serviceType)
        {
            if (WindsorContainer.Kernel.HasComponent(serviceType))
                return WindsorContainer.Resolve(serviceType);

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (WindsorContainer.Kernel.HasComponent(serviceType))
                return WindsorContainer.ResolveAll(serviceType).Cast<object>();
            return new object[0];
        }

        public IDependencyScope BeginScope()
        {
            return new WindsorDependencyScope(this);
        }
    }
}