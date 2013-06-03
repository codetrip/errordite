using System;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace Errordite.Core.IoC
{
    public class WindsorHttpControllerActivator : System.Web.Http.Dispatcher.IHttpControllerActivator
    {
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            //using DependencyScope allows us to use the ScopedLifestyle feature of Windsor
            var controller = (IHttpController)request.GetDependencyScope().GetService(controllerType);
            return controller;
        }
    }
}
