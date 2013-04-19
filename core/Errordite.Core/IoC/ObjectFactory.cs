using System;
using System.Configuration;
using Castle.Facilities.FactorySupport;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Releasers;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Errordite.Core.Exceptions;

namespace Errordite.Core.IoC
{
    public static class ObjectFactory
    {
        private static IWindsorContainer _container;

        private static void Initialise()
        {
            try
            {
                _container = new WindsorContainer(new XmlInterpreter());
            }
            catch (ConfigurationErrorsException ex)
            {
                if (ex.Message == "Could not find section 'castle' in the configuration file associated with this domain.")
                    _container = new WindsorContainer();
                else
                    throw;
            }

            /*
                our transient components are never being released from memory as the container is keeping a reference to them
                setting the release policy to NoTrackingReleasePolicy so windsor never tracks components, the downside of this
                the documentation says this about this policy...
            
                In cases when you don't want Windsor to track your components, you can resort to NoTrackingReleasePolicy. 
                It never tracks the components created, opting out of performing proper component decommission. 
                Its usage is generally discouraged and targeted at limited scenarios of integration with legacy 
                systems or external frameworks that don't allow you to properly release the components.
              
                However we dont have components that require explicit disposal in the container, so setting this as our default policy 
                to see if it has the desired affect on our memory leak.
            */
            _container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
            _container.Kernel.Resolver.AddSubResolver(new ArrayResolver(_container.Kernel));
            _container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel, true));
            _container.AddFacility<FactorySupportFacility>();

            //register our ConfigurationOverrideContainerInitialiser
            _container.Register(Component.For<IContainerInitialiser>()
                .LifeStyle.Transient
                .ImplementedBy(typeof(ConfigurationOverrideContainerInitialiser)));

            //run any initialisers in the container
            foreach(var initialiser in _container.ResolveAll<IContainerInitialiser>())
                initialiser.Init(_container);
        }

        static ObjectFactory()
        {
            Initialise();
        }

        public static IWindsorContainer Container
        {
            get
            {
                return _container;
            }
        }

        public static bool TryGetObject<T>(out T component)
        {
            try
            {
                component = _container.Resolve<T>();
            }
            catch (ComponentNotFoundException)
            {
                component = default(T);
                return false;
            }

            return true;
        }

        public static T GetObject<T>()
        {
            T obj;

            try
            {
                obj = _container.Resolve<T>();
            }
            catch (ComponentNotFoundException ex)
            {
                throw new ErrorditeIoCComponentException<T>(ex);
            }

            return obj;
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetObject<T>(string key)
        {
            T obj;

            try
            {
                obj = _container.Resolve<T>(key);
            }
            catch (ComponentNotFoundException ex)
            {
                throw new ErrorditeIoCComponentException<T>(ex);
            }

            return obj;
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static T GetObject<T>(Type objectType)
        {
            T obj;

            try
            {
                obj = (T)_container.Resolve(objectType);
            }
            catch (ComponentNotFoundException ex)
            {
                throw new ErrorditeIoCComponentException<T>(ex);
            }

            return obj;
        }

        /// <summary>
        /// Uses the object id to check if an object is registered with the IoC container
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasObject(string key)
        {
            return _container.Kernel.HasComponent(key);
        }

        public static bool HasObject(Type type)
        {
            return _container.Kernel.HasComponent(type);
        }

        /// <summary>
        /// Uses the type to check if an object is registered with the IoC container
        /// </summary>
        /// <returns></returns>
        public static bool HasObject<T>()
        {
            return _container.Kernel.HasComponent(typeof(T));
        }

        /// <summary>
        /// Release an object from the container
        /// </summary>
        /// <param name="component"></param>
        public static void Release(object component)
        {
            _container.Release(component);
        }
    }
}
