
using System.Collections.Generic;
using System.Web.Mvc;
using CodeTrip.Core.Caching.Entities;

namespace Errordite.Web.Models.Cache
{
    public class CacheViewModel
    {
        public CacheProfiles Cache { get; set; }
        public string CacheEngine { get; set; }
        public IEnumerable<string> Keys { get; set; }
        public IEnumerable<SelectListItem> Engines { get; set; }
    }

    public class CacheItemViewModel
    {
        public CacheProfiles Cache { get; set; }
        public string CacheEngine { get; set; }
        public string Item { get; set; }
        public string Key { get; set; }
    }
}