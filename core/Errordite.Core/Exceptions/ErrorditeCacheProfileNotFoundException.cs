
using System;
using Errordite.Core.Caching.Entities;
using Errordite.Core.Extensions;

namespace Errordite.Core.Exceptions
{
	[Serializable]
	public class ErrorditeCacheProfileNotFoundException : ErrorditeException
    {
        public ErrorditeCacheProfileNotFoundException(CacheProfiles cacheProfile)
            : base ("CacheProfile with key {0} could not be found in cache configuration.".FormatWith(cacheProfile.ToString()))
        {}
    }
}