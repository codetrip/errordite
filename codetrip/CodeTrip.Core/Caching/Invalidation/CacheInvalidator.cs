using System;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interfaces;
using CodeTrip.Core.Extensions;

namespace CodeTrip.Core.Caching.Invalidation
{
    /// <summary>
    /// Component used to invalidate an item
    /// </summary>
    public class CacheInvalidator : ComponentBase, ICacheInvalidator
    {
        private readonly ICacheConfiguration _cacheConfiguration;
        private ICacheEngine _cacheEngine;

        public CacheInvalidator(ICacheConfiguration cacheConfiguration)
        {
            _cacheConfiguration = cacheConfiguration;
        }

        public void SetCacheEngine(ICacheEngine cacheEngine)
        {
            _cacheEngine = cacheEngine;
        }

        private ICacheEngine GetCacheEngine(CacheProfile cacheProfile)
        {
            return _cacheEngine ?? cacheProfile.Engine;
        }

        public void Invalidate(params CacheInvalidationItem[] items)
        {
            foreach (var item in items)
                InvalidateItem(item);
        }

        public void Invalidate(ICacheInvalidation cacheInvalidator)
        {
            if (cacheInvalidator == null || cacheInvalidator.IgnoreCache)
                return;

            ArgumentValidation.ComponentNotNull(_cacheConfiguration);

            foreach (var item in cacheInvalidator.Items)
                InvalidateItem(item);
        }

        private void InvalidateItem(CacheInvalidationItem item)
        {
            CacheProfile cacheProfile = _cacheConfiguration.GetCacheProfile(item.CacheProfile);

            if (cacheProfile == null)
                throw new InvalidOperationException("CacheProfile with key {0} does not exist.".FormatWith(item.CacheProfile));

            if (item.CacheItemKey.IsNullOrEmpty())
            {
                GetCacheEngine(cacheProfile).Clear(cacheProfile);
            }
            else if (item.IsKeyPrefix)
            {
                Trace("Prefix Invalidation...Cache:={0}, Engine:={1}, Profile:={2}, Prefix:={3}, Timeout:={4}",
                        cacheProfile.ProfileName,
                        cacheProfile.Engine,
                        item.CacheProfile,
                        item.CacheItemKey,
                        cacheProfile.Timeout);

                GetCacheEngine(cacheProfile).Clear(cacheProfile, item.CacheItemKey);
            }
            else
            {
                Trace("Invalidating direct from key..Cache:={0}, Engine:={1}, Profile:={2}, Key:={3}, Timeout:={4}",
                        cacheProfile.ProfileName,
                        cacheProfile.Engine,
                        item.CacheProfile,
                        item.CacheItemKey,
                        cacheProfile.Timeout);

                GetCacheEngine(cacheProfile).Remove(CacheKey.ForProfile(cacheProfile)
                    .WithKey(item.CacheItemKey)
                    .Create());
            }
        }

        protected virtual void Remove(ICacheable cacheableItem, CacheProfile cacheProfile, ICacheEngine cacheEngine)
        {
            cacheEngine.Remove(CacheKey.ForProfile(cacheProfile)
                .WithKey(cacheableItem.CacheItemKey)
                .Create());
        }

        protected virtual void Flush(CacheInvalidationItem item, CacheProfile cacheProfile, ICacheEngine cacheEngine)
        {
            cacheEngine.Clear(cacheProfile, item.CacheItemKey);
        }
    }
}
