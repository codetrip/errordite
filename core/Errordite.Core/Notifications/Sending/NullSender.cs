using System;

namespace Errordite.Core.Notifications.Sending
{
    public class NullSender : IEmailSender
    {
        public void Send(Message message)
        {
            
        }
    }
}