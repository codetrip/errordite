using Errordite.Core;
using Errordite.Core.Notifications.Commands;
using Errordite.Core.Notifications.EmailInfo;

namespace Errordite.Services.Consumers
{
    public class SendEmailConsumer : ComponentBase, IErrorditeConsumer<EmailInfoBase>
    {
        private readonly ISendEmailCommand _sendEmailCommand;

        public SendEmailConsumer(ISendEmailCommand sendEmailCommand)
        {
            _sendEmailCommand = sendEmailCommand;
        }

        public void Consume(EmailInfoBase message)
        {
            _sendEmailCommand.Invoke(new SendEmailRequest
            {
                EmailInfo = message
            });
        }
    }
}