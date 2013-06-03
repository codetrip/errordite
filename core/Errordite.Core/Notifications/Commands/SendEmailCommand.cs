using Errordite.Core;
using Errordite.Core.Interfaces;
using Errordite.Core.Notifications.EmailInfo;
using Errordite.Core.Notifications.Parsing;
using Errordite.Core.Notifications.Rendering;
using Errordite.Core.Notifications.Sending;

namespace Errordite.Core.Notifications.Commands
{
    /// <summary>
    /// Renders an email request down to some text, then sends it.
    /// </summary>
    public class SendEmailCommand : ComponentBase, ISendEmailCommand
    {
        private readonly IEmailRenderer _emailRenderer;
        private readonly IEmailSender _emailSender;
        private readonly IEmailInfoParser _emailInfoParser;
        private readonly ITemplateLocator _templateLocator;

        public SendEmailCommand(IEmailRenderer emailRenderer, 
            IEmailSender emailSender, 
            IEmailInfoParser emailInfoParser, 
            ITemplateLocator templateLocator) 
        {
            _emailRenderer = emailRenderer;
            _templateLocator = templateLocator;
            _emailInfoParser = emailInfoParser;
            _emailSender = emailSender;
        }

        public SendEmailResponse Invoke(SendEmailRequest request)
        {
            Message message;
            Trace("Starting...");

            if (request.EmailInfo is NonTemplatedEmailInfo)
            {
                var emailInfo = request.EmailInfo as NonTemplatedEmailInfo;
                Trace("...Sending non templated email");

                message = new Message
                {
                    To = emailInfo.To,
                    Bcc = emailInfo.Bcc,
                    Body = emailInfo.Body,
                    Cc = emailInfo.Cc,
                    Subject = emailInfo.Subject,
                };
                _emailSender.Send(message);
                Trace("...Sent");
            }
            else
            {
                Trace("...Getting template");
                string template = _templateLocator.GetTemplate(request.EmailInfo);
                Trace("...Parsing email info");
                var emailParams = _emailInfoParser.Parse(request.EmailInfo);
                Trace("...Rendering email");
                message = _emailRenderer.Render(template, emailParams);
                if (!request.SkipSend)
                {
                    Trace("...Sending");
                    _emailSender.Send(message);
                }
                Trace("...Sent");
            }

            return new SendEmailResponse
            {
                Message = message
            };
        }
    }

    public interface ISendEmailCommand : ICommand<SendEmailRequest, SendEmailResponse> 
    { }

    public class SendEmailRequest
    {
        public EmailInfoBase EmailInfo { get; set; }

        public bool SkipSend { get; set; }
    }

    public class SendEmailResponse
    {
        public Message Message { get; set; }
    }
}