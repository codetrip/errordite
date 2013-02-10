using System.Collections.Generic;
using CodeTrip.Core.Caching.Entities;

namespace CodeTrip.Core.Caching.Interfaces
{
    /// <summary>
    /// An entity must implement this to provide an item key - this forms
    /// part of the fully qualified key, also requires the entity to specify a cache profile which
    /// will be used to make up the rest of the key including the cache, engine and expiry
    /// </summary>
    public interface ICacheInvalidation
    {
        /// <summary>
        /// Returns a set of cache items to invalidate
        /// </summary>
        /// <returns></returns>
        IEnumerable<CacheInvalidationItem> Items { get; }
        /// <summary>
        /// Allows commands/queries to override the caching behaviour, items will not be cached or decached if set to true
        /// </summary>
        bool IgnoreCache { get; set; }
    }
}
