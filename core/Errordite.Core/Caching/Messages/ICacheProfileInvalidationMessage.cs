using NServiceBus;

namespace Errordite.Core.Caching.Messages
{
    public interface ICacheProfileInvalidationMessage : IMessage
    {
        string CacheProfileKey { get; set; }
    }

    public class CacheProfileInvalidationMessage : ICacheProfileInvalidationMessage
    {
        public string CacheProfileKey { get; set; }

        public CacheProfileInvalidationMessage(string cacheProfileKey)
        {
            CacheProfileKey = cacheProfileKey;
        }
    }
}