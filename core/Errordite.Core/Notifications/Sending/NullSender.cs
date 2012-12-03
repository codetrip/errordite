using System;

namespace Errordite.Core.Notifications.Sending
{
    public class NullSender : IMessageSender
    {
        public void Send(Message message)
        {
            
        }
    }
}