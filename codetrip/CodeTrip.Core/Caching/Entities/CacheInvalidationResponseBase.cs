using System.Collections.Generic;
using CodeTrip.Core.Caching.Interfaces;

namespace CodeTrip.Core.Caching.Entities
{
    public abstract class CacheInvalidationResponseBase : ICacheInvalidation
    {
        bool ICacheInvalidation.IgnoreCache { get; set; }

        IEnumerable<CacheInvalidationItem> ICacheInvalidation.Items
        {
            get { return GetCacheInvalidationItems(); }
        }

        protected CacheInvalidationResponseBase(bool ignoreCache = false)
        {
            ((ICacheInvalidation)this).IgnoreCache = ignoreCache;
        }

        protected abstract IEnumerable<CacheInvalidationItem> GetCacheInvalidationItems();
    }
}
