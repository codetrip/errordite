using System.Collections.Generic;
using CodeTrip.Core.Exceptions;
using CodeTrip.Core.Extensions;
using System.Linq;

namespace CodeTrip.Core.Caching.Entities
{
    /// <summary>
    /// CacheConfiguration interface, used primarily to facilitate simpler mocking of the CacheConfiguration object in unit tests
    /// </summary>
    public interface ICacheConfiguration
    {
        CacheProfile GetCacheProfile(CacheProfiles cacheProfile);
        IEnumerable<CacheProfile> GetCacheProfiles();
    }

    public class CacheConfiguration : ICacheConfiguration
    {
        private readonly Dictionary<string, CacheProfile> _cacheProfiles;

        public CacheConfiguration(Dictionary<string, CacheProfile> cacheProfiles)
        {
            _cacheProfiles = cacheProfiles;
        }

        public CacheProfile GetCacheProfile(CacheProfiles cacheProfile)
        {
            var cacheProfileKey = cacheProfile.ToString().ToLowerInvariant();

            if (_cacheProfiles == null || !_cacheProfiles.ContainsKey(cacheProfileKey))
                throw new CodeTripCacheException("CacheProfile with key {0} does not exist.".FormatWith(cacheProfileKey));

            return _cacheProfiles[cacheProfileKey];
        }

        public IEnumerable<CacheProfile> GetCacheProfiles()
        {
            return _cacheProfiles.Select(p => p.Value);
        }
    }
}
