using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using CodeTrip.Core.Extensions;
using Errordite.Core.Domain.Error;

namespace Errordite.Core.WebApi
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient httpClient, string requestUri, T obj)
        {
            return httpClient.PutAsync(requestUri, new ObjectContent<T>(obj, new JsonMediaTypeFormatter
            {
                SerializerSettings = WebApiSettings.JsonSerializerSettings
            }));
        }

        public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient httpClient, string requestUri, T obj)
        {
            return httpClient.PostAsync(requestUri, new ObjectContent<T>(obj, new JsonMediaTypeFormatter
            {
                SerializerSettings = WebApiSettings.JsonSerializerSettings
            }));
        }
    }
}