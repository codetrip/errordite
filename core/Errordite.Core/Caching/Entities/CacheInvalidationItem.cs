
namespace Errordite.Core.Caching.Entities
{
    public class CacheInvalidationItem
    {
        public bool IsKeyPrefix { get; set; }
        public string CacheItemKey { get; set; }
        public CacheProfiles CacheProfile { get; set; }

        public CacheInvalidationItem(CacheProfiles cacheProfile, string cacheItemKey, bool isKeyPrefix)
        {
            IsKeyPrefix = isKeyPrefix;
            CacheItemKey = cacheItemKey;
            CacheProfile = cacheProfile;
        }

        public CacheInvalidationItem(CacheProfiles cacheProfile, string cacheItemKey)
            : this(cacheProfile, cacheItemKey, false)
        { }

        public CacheInvalidationItem()
        { }
    }
}
