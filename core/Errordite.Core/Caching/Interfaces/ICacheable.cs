using Errordite.Core.Caching.Entities;

namespace Errordite.Core.Caching.Interfaces
{
    /// <summary>
    /// An entity must implement this to provide an item key - this forms
    /// part of the fully qualified key, also requires the entity to specify a cache profile which
    /// will be used to make up the rest of the key including the cache, engine and expiry
    /// </summary>
    public interface ICacheable
    {
        /// <summary>
        /// Return the unique key for the item being cached, leave null or empty to invalidate entire profile
        /// </summary>
        /// <returns></returns>
        string CacheItemKey { get; }
        /// <summary>
        /// Return the key to the CacheProfile as specified in the CacheConfiguration.CacheProfiles dictionary
        /// </summary>
        CacheProfiles CacheProfile { get; }
        /// <summary>
        /// Allows commands/queries to override the caching behaviour, items will not be cached or decached if set to true
        /// </summary>
        bool IgnoreCache { get; set; }
        /// <summary>
        /// Method to retrieve the item from the cache engine, required as we need type parameters on the ICacheEngine.Get
        /// </summary>
        /// <param name="cacheEngine"></param>
        /// <param name="cacheKey"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        bool GetFromCache(ICacheEngine cacheEngine, CacheKey cacheKey, out object item);
    }
}
