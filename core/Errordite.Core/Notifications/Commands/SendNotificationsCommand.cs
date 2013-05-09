using System.Collections.Generic;
using Errordite.Core.Interfaces;
using Errordite.Core.Messaging;
using Errordite.Core.Paging;
using Errordite.Core.Configuration;
using Errordite.Core.Extensions;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Session;
using Errordite.Core.Session.Actions;
using Errordite.Core.Users.Queries;
using Errordite.Core.Notifications.EmailInfo;
using System.Linq;

namespace Errordite.Core.Notifications.Commands
{
    public class SendNotificationsCommand : SessionAccessBase, ISendNotificationCommand
    {
        private readonly ErrorditeConfiguration _configuration;
        private readonly IGetUsersQuery _getUsersQuery;

        public SendNotificationsCommand(ErrorditeConfiguration configuration, 
            IGetUsersQuery getUsersQuery) 
        {
            _getUsersQuery = getUsersQuery;
            _configuration = configuration;
        }

        public virtual SendNotificationResponse Invoke(SendNotificationRequest request)
        {
            MaybeSendIndividualEmailNotification(request);
            MaybeSendHipChatNotification(request);
            MaybeSendGroupEmailNotification(request);
	        MaybeSendCampfireNotification(request);

            return new SendNotificationResponse();
        }

        private void MaybeSendIndividualEmailNotification(SendNotificationRequest request)
        {
            if (request.EmailInfo == null || request.EmailInfo.To.IsNullOrEmpty())
                return;

            Session.AddCommitAction(new SendMessageCommitAction(
				request.EmailInfo,
                _configuration.GetNotificationsQueueAddress(request.Organisation == null ? "1" : request.Organisation.RavenInstance.FriendlyId)));
        }

        private void MaybeSendGroupEmailNotification(SendNotificationRequest request)
        {
            if (request.Groups.Count == 0 && request.Users.Count == 0)
                return;

            ArgumentValidation.NotNull(request.OrganisationId, "request.OrganisationId");

            var allUsers = _getUsersQuery.Invoke(new GetUsersRequest
            {
                OrganisationId = request.OrganisationId,
                Paging = new PageRequestWithSort(1, _configuration.MaxPageSize)
            }).Users;

            var usersToSendTo = (from userId in request.Users
                                    join userFromDb in allUsers.Items on userId equals userFromDb.Id
                                    select userFromDb)
                .Union(
                    from userFromDb2 in allUsers.Items
                    from userGroup in userFromDb2.GroupIds
                    join groupId in request.Groups on userGroup equals groupId
                    select userFromDb2).Distinct().ToList();

            if (usersToSendTo.Count == 0)
                return;

            request.EmailInfo.To = usersToSendTo.Aggregate(string.Empty, (current, u) => current + (u.Email + ';')).TrimEnd(';');

            Session.AddCommitAction(new SendMessageCommitAction(request.EmailInfo, 
                    _configuration.GetNotificationsQueueAddress(request.Organisation == null ? 
                        "1" :
                        request.Organisation.RavenInstance.FriendlyId)));
        }

		private void MaybeSendCampfireNotification(SendNotificationRequest request)
		{
			if (request.Application == null || 
				request.Application.CampfireRoomId == 0|| 
				request.Organisation.CampfireDetails == null ||
				request.Organisation.CampfireDetails.Token.IsNullOrEmpty() ||
				request.Organisation.CampfireDetails.Company.IsNullOrEmpty())
				return;

			string message = request.EmailInfo.ConvertToNonHtmlMessage(_configuration);

			if (message.IsNullOrEmpty())
				return;

			var campfireMessage = new SendCampfireMessage
			{
				CampfireDetails = request.Organisation.CampfireDetails,
				RoomId = request.Application.CampfireRoomId,
				Message = message
			};

			Session.AddCommitAction(new SendMessageCommitAction(campfireMessage, _configuration.GetNotificationsQueueAddress(request.Organisation == null ? "1" : request.Organisation.RavenInstance.FriendlyId)));
		}

        private void MaybeSendHipChatNotification(SendNotificationRequest request)
        {
			if (request.Application == null || request.Application.HipChatRoomId == 0 || request.Organisation.HipChatAuthToken.IsNullOrEmpty())
                return;

            string message = request.EmailInfo.ConvertToSimpleMessage(_configuration);

            if (message.IsNullOrEmpty())
                return;

            var hipChatMessage = new SendHipChatMessage
			{
				HipChatRoomId = request.Application.HipChatRoomId,
				HipChatAuthToken = request.Organisation.HipChatAuthToken,
				Message = message,
                Colour = request.EmailInfo.HipChatColour,
            };

            Session.AddCommitAction(new SendMessageCommitAction(hipChatMessage,
                _configuration.GetNotificationsQueueAddress(request.Organisation == null ? 
                    "1" :
                    request.Organisation.RavenInstance.FriendlyId)));
        }
    }

    public interface ISendNotificationCommand : ICommand<SendNotificationRequest, SendNotificationResponse> 
    { }

    public class SendNotificationRequest
    {
        public SendNotificationRequest()
        {
            Users = new List<string>();
            Groups = new List<string>();
        }

        public EmailInfoBase EmailInfo { get; set; }
        public string OrganisationId { get; set; }
        public Application Application { get; set; }
        public Organisation Organisation { get; set; }
        public IList<string> Users { get; set; }
        public IList<string> Groups { get; set; }
    }

    public class SendNotificationResponse
    {}
}