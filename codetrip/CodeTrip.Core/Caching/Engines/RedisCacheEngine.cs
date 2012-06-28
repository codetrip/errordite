using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CodeTrip.Core.Auditing.Entities;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Caching.Interfaces;
using CodeTrip.Core.Caching.Resources;
using CodeTrip.Core.Exceptions;
using CodeTrip.Core.Extensions;
using System.Linq;
using CodeTrip.Core.Redis;

namespace CodeTrip.Core.Caching.Engines
{
    /// <summary>
    /// Redis implementation of a cache engine
    /// </summary>
    public class RedisCacheEngine : ComponentBase, ICacheEngine
    {
        private readonly IRedisSession _session;

        public RedisCacheEngine(IRedisSession session, IComponentAuditor auditor = null)
        {
            Auditor = auditor;

            _session = session;
            _session.TryOpenConnection();
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

            if (!_session.EnsureConnectionIsOpen())
            {
                entry = default(T);
                return false;
            }

            var task = _session.Connection.Strings.Get(key.Profile.CacheId, key.Key.ToLowerInvariant());
            var result = _session.Connection.Wait(task);

            //GT: not 100% sure this is correct but added in the result.Length > 0 condition as without it were seeing
            //deserialising to all-null-property instances of T, which should not have been the case
            //In fact fairly sure this is wrong - just shouldn't have been caching this in the first place.. Trello added
            entry = (result != null && result.Length > 0) ? SerializationHelper.ProtobufDeserialize<T>(result) : default(T);
            return (result != null && result.Length > 0);
        }

        public string Name   
        {
            get { return CacheEngines.Redis; }
        }

        public IEnumerable<string> Keys(CacheProfile cacheProfile, string keyPrefix = null)
        {
            if (!_session.EnsureConnectionIsOpen())
                return new string[0];

            var prefix = keyPrefix == null ? "*" : keyPrefix.ToLowerInvariant() + "*";
            var task = _session.Connection.Keys.Find(cacheProfile.CacheId, prefix);
            var result = _session.Connection.Wait(task);
            return result;
        }

        public void Clear(CacheProfile cacheProfile, string keyPrefix = null)
        {
            if (!_session.EnsureConnectionIsOpen())
                return;

            string[] keys = Keys(cacheProfile, keyPrefix).ToArray();

            if (keys.Length > 0)
            {
                var task = _session.Connection.Keys.Remove(cacheProfile.CacheId, keys);
                _session.Connection.Wait(task);
            }
        }

        public void Clear()
        {
            if (!_session.EnsureConnectionIsOpen())
                return;

            for (int id = 1; id < Enum.GetNames(typeof(CacheProfiles)).Length + 1; id++)
            {
                _session.Connection.Wait(_session.Connection.Server.FlushDb(id));
            }
        }

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key"></param>
        public void Remove(CacheKey key)
        {
            ArgumentValidation.NotNull(key, "key");

            if (!_session.EnsureConnectionIsOpen())
                return;

            var task = _session.Connection.Keys.Remove(key.Profile.CacheId, key.Key.ToLowerInvariant());
            _session.Connection.Wait(task);
        }

        /// <summary>
        /// Adds or replaces an item in the cache
        /// </summary>
        /// <param name="cacheItemInfo"></param>
        public void Put(CacheItemInfo cacheItemInfo)
        {
            ArgumentValidation.NotNull(cacheItemInfo, "entry");

            if (!_session.EnsureConnectionIsOpen())
                return;

            try
            {
                var itemKey = cacheItemInfo.Key.Key.ToLowerInvariant();
                var timeout = cacheItemInfo.Key.Profile.GetTimeout();

                Task task;
                if (timeout.HasValue)
                {
                    task = _session.Connection.Strings.Set(cacheItemInfo.Key.Profile.CacheId,
                        itemKey,
                        SerializationHelper.ProtobufSerialize(cacheItemInfo.Item),
                        (long)timeout.Value.TotalSeconds);
                }
                else
                {
                    task = _session.Connection.Strings.Set(cacheItemInfo.Key.Profile.CacheId,
                        cacheItemInfo.Key.Key.ToLowerInvariant(),
                        SerializationHelper.ProtobufSerialize(cacheItemInfo.Item));
                }

                _session.Connection.Wait(task);
            }
            catch (SerializationException e)
            {
                throw new CodeTripCacheException(
                    "Failed to serialize type:={0}, profile:={1}, key:={2}".FormatWith(cacheItemInfo.Item.GetType(), cacheItemInfo.Key.Profile.ProfileName, cacheItemInfo.Key.Key),
                    false,
                    e);
            }
        }
    }
}
