using System;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CodeTrip.Core.Interfaces;

namespace CodeTrip.Core.IoC
{
    /// <summary>
    /// Base class used for all windowr installers, automatically regsiters all IWorkflows, ICommands, IQueries, IRepositories and IMappingDefinitions
    /// </summary>
    public abstract class WindsorInstallerBase : IWindsorInstaller
    {
        public virtual void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //register all IWorkflows
            container.Register(AllTypes.FromAssembly(GetType().Assembly)
                .BasedOn(typeof (IWorkflow<,>))
                    .WithServiceFromInterface(typeof(IWorkflow<,>))
                .If(ExtraTypeFilter)
                .LifestyleTransient());
        }

        protected virtual Predicate<Type> ExtraTypeFilter
        {
            get { return delegate { return true; }; }
        }
    }
}