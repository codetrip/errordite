using System;
using System.IO;
using System.Threading.Tasks;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.IoC;
using Errordite.Events.Service.IoC;
using log4net.Config;
using NServiceBus;

namespace Errordite.Events.Service
{
    public class EndpointConfiguration : IConfigureThisEndpoint, AsA_Server, IWantCustomInitialization
    {
        public void Init()
        {
            Configure.Instance.DefineEndpointName(typeof(EndpointConfiguration).Namespace);
            SetLoggingLibrary.Log4Net(XmlConfigurator.Configure);
            ObjectFactory.Container.Install(new EventsMasterInstaller());
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"config\log4net.config")));

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                try
                {
                    ObjectFactory.GetObject<IComponentAuditor>().Error(GetType(), args.Exception);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.Write(e);
                }

                args.SetObserved();
            };
        }
    }

}