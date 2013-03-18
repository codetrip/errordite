
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Web.Caching;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interfaces;
using CodeTrip.Core.Caching.Resources;
using System.Linq;

namespace CodeTrip.Core.Caching.Engines
{
    /// <summary>
    /// MemoryCache implementation of a cache engine
    /// </summary>
    public class MemoryCacheEngine : ICacheEngine
    {
        private static readonly Dictionary<int, MemoryCache> _caches = new Dictionary<int, MemoryCache>();

        private MemoryCache GetMemoryCache(CacheProfile cacheProfile)
        {
            var cacheId = cacheProfile.CacheId;

            if (!_caches.ContainsKey(cacheId))
            {
                lock (_caches)
                {
                    if (!_caches.ContainsKey(cacheId))
                        _caches.Add(cacheId, new MemoryCache(cacheId.ToString()));
                }
            }

            return _caches[cacheId];
        }

        /// <summary>
        /// Gets an item from the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="entry"></param>
        /// <returns>false if the item was not found in the cache otherwise true</returns>
        public bool Get<T>(CacheKey key, out T entry)
        {
            ArgumentValidation.NotNull(key, "key");

            var itemKey = CreateKey(key.Profile.ProfileName, key.Key);
            var item = GetMemoryCache(key.Profile)[itemKey];

            if (item == null)
            {
                entry = default(T);
                return false;
            }

            entry = (T) item;
		    return true;
        }

        public string Name
        {
            get { return CacheEngines.Memory; }
        }

        /// <summary>
        /// Returns an enumerable list of keys which exist in the specified profile
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Keys(CacheProfile cacheProfile)
        {
            ArgumentValidation.NotNull(cacheProfile, "cacheProfile");
            var ret = Contents(cacheProfile).Select(key => key.Split(new[] { CacheConstants.CacheItemKeyDelimiter }, StringSplitOptions.RemoveEmptyEntries)[1]);
            return ret;
        }

        public IEnumerable<string> Keys(CacheProfile cacheProfile, string keyPrefix)
        {
            ArgumentValidation.NotNull(cacheProfile, "cacheProfile");
            var ret = Contents(cacheProfile, keyPrefix).Select(key => key.Split(new[] { CacheConstants.CacheItemKeyDelimiter }, StringSplitOptions.RemoveEmptyEntries)[1]);
            return ret;
        }

        /// <summary>
        /// Adds or replaces an item in the cache
        /// </summary>
        /// <param name="cacheItemInfo"></param>
        public void Put(CacheItemInfo cacheItemInfo)
        {
            ArgumentValidation.NotNull(cacheItemInfo, "entry");

            var itemKey = CreateKey(cacheItemInfo.Key.Profile.ProfileName, cacheItemInfo.Key.Key);
            var timeout = cacheItemInfo.Key.Profile.GetTimeout();
            var expiry = timeout.HasValue ? DateTime.Now + timeout.Value : Cache.NoAbsoluteExpiration;

            GetMemoryCache(cacheItemInfo.Key.Profile).Set(
                itemKey,
                cacheItemInfo.Item,
                expiry);
        }

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key"></param>
        public void Remove(CacheKey key)
        {
            ArgumentValidation.NotNull(key, "key");
            var itemKey = CreateKey(key.Profile.ProfileName, key.Key);
            GetMemoryCache(key.Profile).Remove(itemKey);
        }

        public void Clear()
        {
            lock (_caches)
            {
                _caches.Clear();
            }
        }

        public void Clear(CacheProfile cacheProfile, string keyPrefix = null)
        {
            ArgumentValidation.NotNull(cacheProfile, "cacheProfile");

            var cache = GetMemoryCache(cacheProfile);

            foreach (var key in Contents(cacheProfile, keyPrefix))
                cache.Remove(key);
        }

        private IEnumerable<string> Contents(CacheProfile cacheProfile, string keyPrefix = null)
        {
            var fullPrefix = string.Format("{0}{1}{2}", cacheProfile.ProfileName, CacheConstants.CacheItemKeyDelimiter, keyPrefix).ToLowerInvariant();

            return from cacheItem in GetMemoryCache(cacheProfile)
                   where cacheItem.Key != null && cacheItem.Key.StartsWith(fullPrefix)
                   select cacheItem.Key;
        }

        /// <summary>
        /// This creates a key suitable for the cache engine to use
        /// to access/store items
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string CreateKey(string cache, string key)
        {
            return string.Format("{0}{2}{1}",
                cache,
                key,
                CacheConstants.CacheItemKeyDelimiter).ToLower();
        }
    }
}
