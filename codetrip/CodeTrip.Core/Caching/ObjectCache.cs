
using System;
using System.Runtime.Caching;

namespace CodeTrip.Core.Caching
{
    /// <summary>
    /// Provides a simple object cache framework for storing small numbers of objects
    /// that are expensive to create and safe to retain a handle to eg: connections/factories
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectCache<T> where T : class
    {
        private readonly ObjectCache _objectCache = new MemoryCache(typeof(T).Name);

        /// <summary>
        /// Attempts to return the cached item for the simple key supplied
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get(string key)
        {
            try
            {
                return (T)_objectCache[key];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// This will reset the cache (remove any cached items). 
        /// </summary>
        public void Clear()
        {
            foreach (var item in _objectCache)
            {
                _objectCache.Remove(item.Key);
            }
        }

        /// <summary>
        /// This will remove an item from the cache
        /// </summary>
        public void Remove(string key)
        {
            _objectCache.Remove(key);
        }

        /// <summary>
        /// This will remove an item from the cache
        /// </summary>
        public void Add(string key, T item, DateTimeOffset expiration)
        {
            _objectCache.Add(key, item, expiration);
        }
    }
}
