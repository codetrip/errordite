using System.Collections.Generic;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CodeTrip.Core.Exceptions;
using NServiceBus;

namespace CodeTrip.Core.ServiceBus
{
    public abstract class NServiceBusInstallerBase : IWindsorInstaller
    {
        private readonly List<Assembly> _assemblies;

        protected NServiceBusInstallerBase(IEnumerable<Assembly> assemblies)
        {
            _assemblies = new List<Assembly>(
                new List<Assembly>
                    {
                        Assembly.Load("NServiceBus"),
                        Assembly.Load("NServiceBus.Core")
                    });

            _assemblies.AddRange(new List<Assembly>(assemblies));
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            try
            {
                AdditionalContainerActions(container, store);

                Configure.With(_assemblies)
                    .CastleWindsorBuilder(container)
                        .XmlSerializer()
                    .Log4Net()
                    .MsmqTransport()
                    .MsmqSubscriptionStorage("Errordite")
                    .IsTransactional(IsTransactional)
                    .PurgeOnStartup(PurgeOnStartup)
                    .UnicastBus()
                        .ImpersonateSender(false)
                        .ConditionalLoadMessageHandlers(LoadMessageHandlers)
                    .CreateBus()
                    .Start();
            }
            catch (ReflectionTypeLoadException ex)
            {
                throw new CodeTripMoreUsefulReflectionTypeLoadException(ex);
            }
        }

        protected virtual void AdditionalContainerActions(IWindsorContainer container, IConfigurationStore store)
        {}

        protected abstract bool IsTransactional { get; }
        protected abstract bool LoadMessageHandlers { get; }
        protected abstract bool PurgeOnStartup { get; }
        protected abstract bool SendOnly { get; }
    }

    public abstract class NServiceBusClientInstaller : NServiceBusInstallerBase
    {
        protected NServiceBusClientInstaller(IEnumerable<Assembly> assemblies)
            : base(assemblies)
        { }

        protected override bool IsTransactional
        {
            get { return false; } //http://www.nservicebus.com/Transactions.aspx
        }

        protected override bool LoadMessageHandlers
        {
            get { return false; }
        }

        protected override bool PurgeOnStartup
        {
            get { return true; }
        }

        protected override bool SendOnly
        {
            get { return false; }
        }
    }

    public abstract class NServiceBusServerInstaller : NServiceBusInstallerBase
    {
        protected NServiceBusServerInstaller(IEnumerable<Assembly> assemblies)
            : base(assemblies)
        { }

        protected override bool IsTransactional
        {
            get { return true; } //http://www.nservicebus.com/Transactions.aspx
        }

        protected override bool LoadMessageHandlers
        {
            get { return true; }
        }

        protected override bool PurgeOnStartup
        {
            get { return false; }
        }

        protected override bool SendOnly
        {
            get { return false; }
        }        
    }
}