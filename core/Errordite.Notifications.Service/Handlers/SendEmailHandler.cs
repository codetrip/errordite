using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.ServiceBus;

namespace Errordite.Notifications.Service.Handlers
{
    public class SendEmailHandler : MessageHandlerSessionBase<EmailInfoBase>
    {
        private readonly ISendEmailCommand _sendEmailCommand;

        public SendEmailHandler(ISendEmailCommand sendEmailCommand)
        {
            _sendEmailCommand = sendEmailCommand;
        }

        protected override void HandleMessage(EmailInfoBase message)
        {
            _sendEmailCommand.Invoke(new SendEmailRequest
            {
                EmailInfo = message
            });
        }
    }
}
