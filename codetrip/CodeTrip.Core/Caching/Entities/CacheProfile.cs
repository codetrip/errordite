using System;
using System.Threading;
using CodeTrip.Core.Caching.Interfaces;

namespace CodeTrip.Core.Caching.Entities
{
    public class CacheProfile
    {
        private static readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random());

        private CacheProfile()
        {
            CacheId = 0;
        }

        /// <summary>
        /// Name of the cache where the item lives, this is a logical (httpruntime) and physical (velocity)
        /// partition of items.
        /// </summary>
        public string ProfileName { get; set; }
        /// <summary>
        /// The cache engine - HttpRuntime, Velocity, Memcached etc
        /// </summary>
        public ICacheEngine Engine { get; set; }
        /// <summary>
        /// Timeout of the item, item will be removed from cache at the end of this period
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// If set, this varies specified timeout by X% into the future (this prevents all
        /// warm-up stuff falling out of the cache simultaneously).
        /// 
        /// If not explicitly set, a default of 10% is used.
        /// </summary>
        public int? TimeoutVariancePercentage { get; set; }

        /// <summary>
        /// Specific Id for a cache, used to identify it
        /// </summary>
        public int CacheId { get; set; }

        /// <summary>
        /// Gets the time, with respect to the variance.
        /// </summary>
        /// <param name="varyWithinBounds">Defaults to true - if set the timeout is varied by up to a certain percentage (default=10).</param>
        public TimeSpan? GetTimeout(bool varyWithinBounds = true)
        {
            if (!Timeout.HasValue)
                return null;
                
            int maxVarianceSeconds = (int)(Timeout.Value.TotalSeconds *(TimeoutVariancePercentage ?? 10))/100;

            int varianceSeconds = _random.Value.Next(0, maxVarianceSeconds);

            return Timeout.Value.Add(new TimeSpan(0, 0, varianceSeconds));
        }

        public CacheProfile(int cacheId, string profileName, ICacheEngine engine)
            :this()
        {
            ProfileName = profileName;
            Engine = engine;
            CacheId = cacheId;
        }

        public CacheProfile(int cacheId, string profileName, ICacheEngine engine, string timeout)
            : this()
        {
            ProfileName = profileName;
            Engine = engine;
            Timeout = TimeSpan.Parse(timeout);
            CacheId = cacheId;
        }
    }
}
