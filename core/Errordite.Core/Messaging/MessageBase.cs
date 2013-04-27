
using Errordite.Core.Domain.Organisation;
using Newtonsoft.Json;

namespace Errordite.Core.Messaging
{
    public class MessageBase
    {
        public string OrganisationId { get; set; }
        
        [JsonIgnore]
        public Organisation Organisation { get; set; }
    }
}
