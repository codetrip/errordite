
namespace CodeTrip.Core.Caching.Entities
{
    public class CacheKey : IFluentCacheKey
    {
        /// <summary>
        /// Set of properties to define things such as which cache and region an item lives in and its expiry
        /// </summary>
        public CacheProfile Profile { get; set; }
        /// <summary>
        /// uniquely identifies the object within the specified cache profile
        /// </summary>
        public string Key { get; set; }
        
        public string Summary
        {
            get
            {
                return string.Format("Cache:={0}, Engine:={1}, Timeout:={2}, Key:={3}",
                    Profile.ProfileName,
                    Profile.Engine,
                    Profile.Timeout,
                    Key);
            }
        }

        IFluentCacheKey IFluentCacheKey.WithKey(string key)
        {
            Key = key;
            return this;
        }

        CacheKey IFluentCacheKey.Create()
        {
            return this;
        }

        public static IFluentCacheKey ForProfile(CacheProfile cacheProfile)
        {
            return new CacheKey { Profile = cacheProfile };
        }
    }

    /// <summary>
    /// Fluent interface to construct CacheKey objects
    /// </summary>
    public interface IFluentCacheKey
    {
        IFluentCacheKey WithKey(string key);
        CacheKey Create();
    }
}
