using System;
using System.Collections.Generic;

namespace CodeTrip.Core
{
    public static class Memo
    {
        public static Memo<TKey, TValue> Init<TKey, TValue>(Func<TKey, TValue> getter)
        {
            return new Memo<TKey, TValue>(getter);
        }
    }

    /// <summary>
    /// If getting object for the 1st time, use the delegate, otherwise
    /// return the previously gotten object.
    /// </summary>
    public class Memo<TKey, TValue>
    {
        private readonly Func<TKey, TValue> _getter;
        private readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

        public Memo(Func<TKey, TValue> getter)
        {
            _getter = getter;
        }

        public int CacheSize { get { return _cache.Count; } }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!_cache.TryGetValue(key, out value))
                {
                    value = _getter(key);
                    _cache[key] = value;
                }
                return value;
            }
        }
    }
}