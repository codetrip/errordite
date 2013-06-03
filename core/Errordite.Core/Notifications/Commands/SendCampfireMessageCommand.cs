using Errordite.Core.Domain.Organisation;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using GrayHills.Matches;

namespace Errordite.Core.Notifications.Commands
{
    /// <summary>
    /// Renders an CampfireMessage request down to some text, then sends it.
    /// </summary>
    public class SendCampfireMessageCommand : ComponentBase, ISendCampfireMessageCommand
    {
        public SendCampfireMessageResponse Invoke(SendCampfireMessageRequest request)
        {
			if (request.CampfireDetails != null && 
				request.CampfireDetails.Company.IsNotNullOrEmpty() && 
				request.CampfireDetails.Token.IsNotNullOrEmpty() &&
				request.RoomId > 0)
            {
				Trace("Sending campfire message, Token:={0}, Company:={1}, Room:={2}", 
					request.CampfireDetails.Token, 
					request.CampfireDetails.Company, 
					request.RoomId);

				var site = new Site(request.CampfireDetails.Company, new CampfireCredentials(request.CampfireDetails.Token));
				var room = site.GetRoom(request.RoomId);
				room.Say(request.Message);

				Trace("Successfully sent message:={0}", request.Message);
            }
			else
			{
				Trace("Invalid campfire credentials, not sending message");
			}
            
            return new SendCampfireMessageResponse();
        }
    }

    public interface ISendCampfireMessageCommand : ICommand<SendCampfireMessageRequest, SendCampfireMessageResponse> 
    { }

    public class SendCampfireMessageRequest
    {
		public string Message { get; set; }
		public int RoomId { get; set; }
		public CampfireDetails CampfireDetails { get; set; }
    }

    public class SendCampfireMessageResponse
    {}
}