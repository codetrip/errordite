namespace Errordite.Core.Notifications.Sending
{
    public interface IEmailSender
    {
        void Send(Message message);
    }
}