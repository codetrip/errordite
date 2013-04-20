using System.Collections.Generic;
using Errordite.Core.Interfaces;
using Errordite.Core.Messaging;
using Errordite.Core.Paging;
using Errordite.Core.Configuration;
using Errordite.Core.Extensions;
using Errordite.Core.Domain.Organisation;
using Errordite.Core.Session;
using Errordite.Core.Users.Queries;
using Errordite.Core.Notifications.EmailInfo;
using System.Linq;
using SessionAccessBase = Errordite.Core.Session.SessionAccessBase;

namespace Errordite.Core.Notifications.Commands
{
    public class SendNotificationsCommand : SessionAccessBase, ISendNotificationCommand
    {
        private readonly ErrorditeConfiguration _configuration;
        private readonly IGetUsersQuery _getUsersQuery;
        private readonly ISendEmailCommand _sendEmailCommand;
        private readonly ISendHipChatMessageCommand _sendHipChatMessageCommand;

        public SendNotificationsCommand(ErrorditeConfiguration configuration, 
            ISendEmailCommand sendEmailCommand, 
            IGetUsersQuery getUsersQuery, 
            ISendHipChatMessageCommand sendHipChatMessageCommand) 
        {
            _sendEmailCommand = sendEmailCommand;
            _getUsersQuery = getUsersQuery;
            _sendHipChatMessageCommand = sendHipChatMessageCommand;
            _configuration = configuration;
        }

        public virtual SendNotificationResponse Invoke(SendNotificationRequest request)
        {
            MaybeSendIndividualEmailNotification(request);
            MaybeSendHipChatNotification(request);
            MaybeSendGroupEmailNotification(request);

            return new SendNotificationResponse();
        }

        private void MaybeSendIndividualEmailNotification(SendNotificationRequest request)
        {
            if (request.EmailInfo == null || request.EmailInfo.To.IsNullOrEmpty())
                return;

            if (!_configuration.ServiceBusEnabled)
            {
                _sendEmailCommand.Invoke(new SendEmailRequest { EmailInfo = request.EmailInfo });
            }
            else
            {
                Session.AddCommitAction(new SendMessageCommitAction("Send {0}".FormatWith(request.EmailInfo.GetType().Name), request.EmailInfo, _configuration.NotificationsQueueName));
            }
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

            //var alertInfo = request.EmailInfo as IAlertInfo;
            //if (alertInfo != null)
            //{
            //    foreach (var user in usersToSendTo) //hopefully this isn't too large!
            //    {
            //        Store(new UserAlert
            //        {
            //            UserId = user.Id,
            //            Message = alertInfo.MessageTemplate,
            //            Replacements = alertInfo.Replacements,
            //            Type = request.EmailInfo.GetType().Name,
            //        });
            //    }
            //}

            if (!_configuration.ServiceBusEnabled)
            {
                _sendEmailCommand.Invoke(new SendEmailRequest { EmailInfo = request.EmailInfo });
            }
            else
            {
                Session.AddCommitAction(new SendMessageCommitAction("Send {0}".FormatWith(request.EmailInfo.GetType().Name), request.EmailInfo, _configuration.NotificationsQueueName));
            }
        }

        private void MaybeSendHipChatNotification(SendNotificationRequest request)
        {
            if (request.Application == null || request.Application.HipChatAuthToken.IsNullOrEmpty() || !request.Application.HipChatRoomId.HasValue)
                return;

            string message = request.EmailInfo.ConvertToSimpleMessage(_configuration);

            if (message.IsNullOrEmpty())
                return;

            if (!_configuration.ServiceBusEnabled)
            {
                _sendHipChatMessageCommand.Invoke(new SendHipChatMessageRequest
                {
                    HipChatRoomId = request.Application.HipChatRoomId.Value,
                    HipChatAuthToken = request.Application.HipChatAuthToken,
                    Message = request.EmailInfo.ConvertToSimpleMessage(_configuration)
                });
            }
            else
            {
                var hipChatMessage = new SendHipChatMessage
                {
                    HipChatRoomId = request.Application.HipChatRoomId.Value,
                    HipChatAuthToken = request.Application.HipChatAuthToken,
                    Message = request.EmailInfo.ConvertToSimpleMessage(_configuration)
                };

                Session.AddCommitAction(new SendMessageCommitAction("Send {0}".FormatWith(hipChatMessage.GetType().Name), hipChatMessage, _configuration.NotificationsQueueName));
            }
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
        public IList<string> Users { get; set; }
        public IList<string> Groups { get; set; }
    }

    public class SendNotificationResponse
    {}
}