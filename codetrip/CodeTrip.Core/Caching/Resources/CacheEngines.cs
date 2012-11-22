namespace CodeTrip.Core.Caching.Resources
{
    public static class CacheEngines
    {
        /// <summary>
        /// Null Cache Engine
        /// </summary>
        public const string None = "none";

        /// <summary>
        /// Redis Cache Engine
        /// </summary>
        public const string Redis = "redis";

        /// <summary>
        /// Hybrid Cache Engine, combination of In Memory and Redis
        /// </summary>
        public const string RedisMemoryHybrid = "redis-memory-hybrid";

        /// <summary>
        /// In memory cache implementation
        /// </summary>
        public const string Memory = "memory";
    }
}