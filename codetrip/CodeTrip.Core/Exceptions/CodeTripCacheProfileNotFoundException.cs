
using System;
using CodeTrip.Core.Caching.Entities;
using CodeTrip.Core.Extensions;

namespace CodeTrip.Core.Exceptions
{
	[Serializable]
	public class CodeTripCacheProfileNotFoundException : CodeTripException
    {
        public CodeTripCacheProfileNotFoundException(CacheProfiles cacheProfile)
            : base ("CacheProfile with key {0} could not be found in cache configuration.".FormatWith(cacheProfile.ToString()))
        {}
    }
}