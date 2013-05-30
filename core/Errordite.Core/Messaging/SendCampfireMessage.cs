
using Errordite.Core.Domain.Organisation;

namespace Errordite.Core.Messaging
{
    public class SendCampfireMessage : MessageBase
    {
		public string Message { get; set; }
		public int RoomId { get; set; }
        public CampfireDetails CampfireDetails { get; set; }
    }
}
