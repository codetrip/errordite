
namespace Errordite.Core.Messaging
{
    public interface IMessageSender
    {
        void Send<T>(T message, string destination) where T : MessageBase;
    }
}
