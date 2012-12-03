
using System.Collections.Generic;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interfaces;
using System.Linq;
using CodeTrip.Core.Caching.Resources;

namespace CodeTrip.Core.Caching.Engines
{
    /// <summary>
    /// Designed to operate with two cache engines, a high and low level cache.
    /// </summary>
    public class RedisMemoryHybridCacheEngine : ICacheEngine
    {
        private readonly ICacheEngine _memoryCacheEngine;
        private readonly ICacheEngine _redisCacheEngine;

        public RedisMemoryHybridCacheEngine(ICacheEngine memoryCache, ICacheEngine redisCache)
        {
            _memoryCacheEngine = memoryCache;
            _redisCacheEngine = redisCache;
        }

        /// <summary>
        /// Returns just those keys which exist in both caches
        /// </summary>
        /// <param name="cacheProfile">Name of the cache.</param>
        /// <returns></returns>
        public IEnumerable<string> Keys(CacheProfile cacheProfile)
        {
            return _memoryCacheEngine.Keys(cacheProfile).Union(_redisCacheEngine.Keys(cacheProfile));
        }

        public IEnumerable<string> Keys(CacheProfile cacheProfile, string keyPrefix)
        {
            return _memoryCacheEngine.Keys(cacheProfile, keyPrefix).Union(_redisCacheEngine.Keys(cacheProfile, keyPrefix));
        }

        public void Clear()
        {
            _redisCacheEngine.Clear();
            _memoryCacheEngine.Clear();
        }

        public void Clear(CacheProfile cacheProfile, string keyPrefix = null)
        {
            _redisCacheEngine.Clear(cacheProfile, keyPrefix);
            _memoryCacheEngine.Clear(cacheProfile, keyPrefix);
        }

        public void Put(CacheItemInfo cacheItem)
        {
            _memoryCacheEngine.Put(cacheItem);
            _redisCacheEngine.Put(cacheItem);
        }

        public void Remove(CacheKey cacheKey)
        {
            _redisCacheEngine.Remove(cacheKey);
            _memoryCacheEngine.Remove(cacheKey);
        }

        public bool Get<T>(CacheKey cacheKey, out T cacheItem)
        {
            if (_memoryCacheEngine.Get(cacheKey, out cacheItem))
            {
                return true;
            }

            if (_redisCacheEngine.Get(cacheKey, out cacheItem))
            {
                //if the item is in the redis cache but not the memory cache, add it to the memory level
                _memoryCacheEngine.Put(new CacheItemInfo
                {
                    Item = cacheItem,
                    Key = cacheKey
                });

                return true;
            }

            return false;
        }

        public string Name
        {
            get 
            {
                return CacheEngines.RedisMemoryHybrid;
            }
        }
    }
}
