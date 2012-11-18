using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Castle.MicroKernel.Lifestyle;

namespace Errordite.Core.IoC
{
    public class WindsorDependencyScope : IDependencyScope
    {
        private readonly WindsorDependencyResolver _dependencyResolver;
        private readonly IDisposable _windsorScope;

        public WindsorDependencyScope(WindsorDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
            _windsorScope = dependencyResolver.WindsorContainer.BeginScope();
            //creating a scope just begins a scope in the Windsor container, but continues using the same resolver
            //to actually get the services.  
        }

        public void Dispose()
        {
            _windsorScope.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return _dependencyResolver.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _dependencyResolver.GetServices(serviceType);
        }
    }
}