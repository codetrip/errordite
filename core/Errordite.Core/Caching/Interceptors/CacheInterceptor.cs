using System;
using Castle.DynamicProxy;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Caching.Interfaces;
using Errordite.Core.Extensions;

namespace Errordite.Core.Caching.Interceptors
{
    public interface ICacheInterceptor : IInterceptor
    {

    }

    /// <summary>
    /// Intercepts the method being invoked and attempts to cache the results or return the cached results if the item
    /// is located in the cache engine. The first argument of the method must implement ICacheable in order to inform
    /// the code of how and where the item should be cached.
    /// </summary>
    public class CacheInterceptor : ComponentBase, ICacheInterceptor
    {
        public const string IoCName = "DefaultCacheInterceptor";

        private readonly ICacheConfiguration _cacheConfiguration;

        public CacheInterceptor(ICacheConfiguration cacheConfiguration)
        {
            _cacheConfiguration = cacheConfiguration;
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Arguments == null || invocation.Arguments.Length == 0 || invocation.Arguments[0] as ICacheable == null)
            {
                Trace("Caching is not supported by this method, the first argument passed to the method must implement ICacheable.");
                invocation.Proceed();
                return;
            }

            var cacheableRequest = (ICacheable)invocation.Arguments[0];

            if (cacheableRequest.IgnoreCache)
            {
                Trace("Caching has been overridden for this method {0}, returning without caching.", invocation.Method.Name);
                invocation.Proceed();
                return;
            }

            CacheProfile cacheProfile = _cacheConfiguration.GetCacheProfile(cacheableRequest.CacheProfile);

            if (cacheProfile == null)
                throw new InvalidOperationException("Cache profile with the key {0} does not exist".FormatWith(cacheableRequest.CacheProfile));

            CacheKey cacheKey = CacheKey.ForProfile(cacheProfile)
                .WithKey(cacheableRequest.CacheItemKey)
                .Create();

            bool found;
            object response = null;

            try
            {
                found = cacheableRequest.GetFromCache(cacheProfile.Engine, cacheKey, out response);
            }
            catch (Exception ex)
            {
                Error(ex);
                found = false;
            }

			if (!found)
            {
                Trace("CACHE MISS...CacheId:={0}, Engine:={1}, Profile:={2}, Key:={3}, Timeout:={4}",
                    cacheProfile.CacheId,
                    cacheProfile.Engine.Name,
                    cacheableRequest.CacheProfile,
                    cacheableRequest.CacheItemKey,
                    cacheProfile.Timeout);

                invocation.Proceed();

                try
                {
                    cacheProfile.Engine.Put(new CacheItemInfo
                    {
                        Item = invocation.ReturnValue,
                        Key = cacheKey
                    });
                }
                catch (Exception e)
                {
                    Error(e);
                }
            }
            else
            {
                invocation.ReturnValue = response;

                Trace("CACHE HIT...Type:={5}, CacheId:={0}, Engine:={1}, Profile:={2}, Key:={3}, Timeout:={4}",
                    cacheProfile.CacheId,
                    cacheProfile.Engine.Name,
                    cacheableRequest.CacheProfile,
                    cacheableRequest.CacheItemKey,
                    cacheProfile.Timeout,
                    response.GetType().Name);
            }
        }
    }
}
