
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.Caching.Engines;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interfaces;
using CodeTrip.Core.IoC;
using CodeTrip.Core.Redis;

namespace CodeTrip.Core.Test
{
    public class NullCacheInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<ICacheConfiguration>().ImplementedBy<NullCacheConfiguration>());
        }
    }

    public class NullCacheConfiguration : ICacheConfiguration
    {
        public CacheProfile GetCacheProfile(CacheProfiles cacheProfile)
        {
            return new CacheProfile(1, "", new NullCacheEngine());
        }

        public IEnumerable<CacheProfile> GetCacheProfiles()
        {
            return new[] { new CacheProfile(1, "", new NullCacheEngine()) };
        }
    }

    public class RedisCacheInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<ICacheConfiguration>().ImplementedBy<NullCacheConfiguration>());
            container.Register(Component.For<RedisConfiguration>().Instance(new RedisConfiguration
            {
                Endpoint = "127.0.0.1"
            }));
            container.Register(Component.For<IRedisFactory>().ImplementedBy<RedisFactory>().LifestyleSingleton());
            container.Register(Component.For<IRedisSession>().ImplementedBy<RedisSession>());
            container.Register(Component.For<ICacheEngine>().ImplementedBy<RedisCacheEngine>().UsingFactoryMethod(
                kernel =>
                kernel.Resolve<IRedisFactory>().Create()).LifestyleSingleton());
        }
    }

    public interface IRedisFactory
    {
        ICacheEngine Create();
    }

    public class RedisFactory : IRedisFactory
    {
        public ICacheEngine Create()
        {
            return new RedisCacheEngine(ObjectFactory.GetObject<IRedisSession>(), ObjectFactory.GetObject<IComponentAuditor>());
        }
    }

    public class RedisCacheConfiguration : ICacheConfiguration
    {
        public CacheProfile GetCacheProfile(CacheProfiles cacheProfile)
        {
            return new CacheProfile(1, "test", ObjectFactory.GetObject<ICacheEngine>());
        }

        public IEnumerable<CacheProfile> GetCacheProfiles()
        {
            return new[] { new CacheProfile(1, "test", ObjectFactory.GetObject<ICacheEngine>()) };
        }
    }
}
