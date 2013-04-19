using NServiceBus;

namespace Errordite.Core.Caching.Messages
{
    public interface ICacheInvalidationMessage : IMessage
    {
        string CacheProfileKey { get; set; }
        string CacheItemKey { get; set; }
        string Regex { get; set; }
    }

    public class CacheInvalidationMessage : ICacheInvalidationMessage
    {
        public string CacheProfileKey { get; set; }
        public string CacheItemKey { get; set; }
        public string Regex { get; set; }

        public CacheInvalidationMessage(string cacheProfileKey, string cacheItemKey) : 
            this(cacheProfileKey, cacheItemKey, string.Empty)
        { }

        public CacheInvalidationMessage(string cacheProfileKey, string cacheItemKey, string regex)
        {
            CacheProfileKey = cacheProfileKey;
            CacheItemKey = cacheItemKey;
            Regex = regex;
        }
    }
}