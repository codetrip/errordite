namespace Errordite.Core.Notifications.Sending
{
    public interface IMessageSender
    {
        void Send(Message message);
    }
}