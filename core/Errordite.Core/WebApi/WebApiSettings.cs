using Newtonsoft.Json;

namespace Errordite.Core.WebApi
{
    public static class WebApiSettings
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            //TODO: may be able to get away with TypeNameHandling.Objects. But we need at least this as the rule collection is an interface
            TypeNameHandling = TypeNameHandling.All
        };

        /// <summary>
        /// These are the settings we use for both client and server serialization.  
        /// </summary>
        public static JsonSerializerSettings JsonSerializerSettings
        {
            get { return _jsonSerializerSettings; }
        }
    }
}