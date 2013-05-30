
using Errordite.Core.Caching.Interfaces;

namespace Errordite.Core.Caching.Entities
{
    public abstract class CacheableRequestBase<T> : ICacheable
    {
		bool ICacheable.IgnoreCache { get; set; }

	    string ICacheable.CacheItemKey
        {
            get { return GetCacheKey(); }
        }

        CacheProfiles ICacheable.CacheProfile
        {
            get { return GetCacheProfile(); }
        }

        bool ICacheable.GetFromCache(ICacheEngine cacheEngine, CacheKey cacheKey, out object item)
        {
            T cachedItem;
            bool found = cacheEngine.Get(cacheKey, out cachedItem);
            item = cachedItem;
            return found;
        }

        protected abstract string GetCacheKey();
        protected abstract CacheProfiles GetCacheProfile();
    }
}
