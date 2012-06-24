
using CodeTrip.Core.Caching.Entities;

namespace CodeTrip.Core.Caching.Interfaces
{
    public interface ICacheInvalidator
    {
        void Invalidate(params CacheInvalidationItem[] items);
        void Invalidate(ICacheInvalidation cacheInvalidation);
        void SetCacheEngine(ICacheEngine cacheEngine);
    }
}
