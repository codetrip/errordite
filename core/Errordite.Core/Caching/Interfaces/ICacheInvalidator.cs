
using Errordite.Core.Caching.Entities;

namespace Errordite.Core.Caching.Interfaces
{
    public interface ICacheInvalidator
    {
        void Invalidate(params CacheInvalidationItem[] items);
        void Invalidate(ICacheInvalidation cacheInvalidation);
        void SetCacheEngine(ICacheEngine cacheEngine);
    }
}
