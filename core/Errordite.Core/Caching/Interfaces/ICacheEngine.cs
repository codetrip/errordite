
using System.Collections.Generic;
using Errordite.Core.Caching.Entities;

namespace Errordite.Core.Caching.Interfaces
{
    /// <summary>
    /// ICacheEngine represents a object capable of caching data
    /// </summary>
    public interface ICacheEngine
    {
        IEnumerable<string> Keys(CacheProfile cacheProfile, string keyPrefix = null);

        void Clear();
        void Clear(CacheProfile cacheProfile, string keyPrefix = null);

        void Put(CacheItemInfo cacheItemInfo);
        void Remove(CacheKey cacheKey);
        bool Get<T>(CacheKey cacheKey, out T cacheItem);

        string Name { get; }
    }
}
