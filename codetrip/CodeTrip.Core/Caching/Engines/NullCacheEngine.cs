using System.Collections.Generic;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interfaces;
using CodeTrip.Core.Caching.Resources;

namespace CodeTrip.Core.Caching.Engines
{
    public class NullCacheEngine : ICacheEngine
    {
        public IEnumerable<string> Keys(CacheProfile cacheProfile)
        {
            return new string[0];
        }

        public IEnumerable<string> Keys(CacheProfile cacheProfile, string keyPrefix)
        {
            return new string[0];
        }

        public void Clear()
        {}

        public void Clear(CacheProfile cacheProfile, string keyPrefix = null)
        {}

        public void Put(CacheItemInfo cacheItemInfo)
        {}

        public void Remove(CacheKey cacheKey)
        {}

        public bool Get<T>(CacheKey cacheKey, out T cacheItem)
        {
            cacheItem = default(T);
            return false;
        }

        public string Name
        {
            get { return CacheEngines.None; }
        }

        public CacheItemInfo GetInfo(CacheKey cacheKey)
        {
            return null;
        }
    }
}
