using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Releasers;
using Castle.Windsor;
using Castle.MicroKernel;
using Castle.Core;
using NServiceBus;
using NServiceBus.ObjectBuilder.Common;

namespace Errordite.Utils.NServiceBusCastleBridge
{
    /// <summary>
    ///Castle Windsor implementaton of IContainer.
    /// </summary>
    public class WindsorObjectBuilder : IContainer
    {
        /// <summary>
        ///The container itself.
        /// </summary>
        public IWindsorContainer Container { get; set; }

        /// <summary>
        ///Instantites the class with a new WindsorContainer setting the NoTrackingReleasePolicy.
        /// </summary>
        public WindsorObjectBuilder()
        {
            Container = new WindsorContainer();
            Container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
        }

        /// <summary>
        ///Instantiates the class saving the given container.
        /// </summary>
        /// <param name="container"></param>
        public WindsorObjectBuilder(IWindsorContainer container)
        {
            Container = container;
        }

       void IContainer.Configure(Type component, DependencyLifecycle dependencyLifecycle)
       {
           var handler = GetHandlerForType(component);

           if (handler == null)
           {
               var lifestyle = GetLifestyleTypeFrom(dependencyLifecycle);

               var reg = Component.For(GetAllServiceTypesFor(component)).ImplementedBy(component);
               reg.LifeStyle.Is(lifestyle);

               Container.Kernel.Register(reg);
           }
       }

       void IContainer.ConfigureProperty(Type component, string property, object value)
       {
            var registration = Container.Kernel.GetAssignableHandlers(component).Select(x => x.ComponentModel).SingleOrDefault();

            if (registration==null)
                throw new InvalidOperationException("Cannot configure property for a type which hadn't been configured yet. Please call 'Configure' first.");

            var propertyInfo = component.GetProperty(property);

            registration.AddProperty(
                new PropertySet(propertyInfo, 
                    new DependencyModel(property, propertyInfo.PropertyType, false, true, value )));
       }

       void IContainer.RegisterSingleton(Type lookupType, object instance)
       {
           Container.Register(Component.For(lookupType).Named(Guid.NewGuid().ToString()).Instance(instance));
       }

       bool IContainer.HasComponent(Type componentType)
       {
           return Container.Kernel.HasComponent(componentType);
       }

       object IContainer.Build(Type typeToBuild)
       {
            try
            {
                return Container.Resolve(typeToBuild);
            }
            catch(ComponentNotFoundException)
            {
                return null;
            }
       }

       IContainer IContainer.BuildChildContainer()
       {
           var container = new WindsorObjectBuilder();
           Container.AddChildContainer(container.Container);
           return container;
       }

        IEnumerable<object> IContainer.BuildAll(Type typeToBuild)
       {
            return Container.ResolveAll(typeToBuild).Cast<object>();
       }

       private static LifestyleType GetLifestyleTypeFrom(DependencyLifecycle callModel)
       {
            switch(callModel)
            {
                case DependencyLifecycle.InstancePerCall:
                case DependencyLifecycle.InstancePerUnitOfWork: 
                    return LifestyleType.Transient;
                case DependencyLifecycle.SingleInstance: 
                    return LifestyleType.Singleton;
            }

            return LifestyleType.Transient;
       }

       private static IEnumerable<Type> GetAllServiceTypesFor(Type t)
       {
            if(t == null)
                return new List<Type>();

            var result = new List<Type>(t.GetInterfaces()) { t };

            foreach (var interfaceType in t.GetInterfaces())
                result.AddRange(GetAllServiceTypesFor(interfaceType));

            return result;
       }

       private IHandler GetHandlerForType(Type concreteComponent)
       {
            return Container.Kernel.GetAssignableHandlers(typeof(object)).FirstOrDefault(h => h.ComponentModel.Implementation == concreteComponent);
       }

       public void Dispose()
       {}
   }
}